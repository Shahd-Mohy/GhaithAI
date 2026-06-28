import { Injectable, OnDestroy } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { SessionSignalrService } from './session-signalr.service';
import { WebrtcService } from './webrtc.service';
import { AudioRecorderService } from './audio-recorder.service';

// ─── WHY @Injectable() with no scope? ────────────────────────────────────────
// If this were providedIn:'root', the doctor's SessionRoomComponent and the
// patient's PatientSessionRoomComponent would share ONE instance — the same
// sessionId, role, RTCPeerConnection, and HubConnection.
// The second component to call startCall() would silently overwrite the first.
//
// By declaring providers:[SessionOrchestratorService, SessionSignalrService,
// WebrtcService, AudioRecorderService] on EACH component, Angular gives each
// component its own completely isolated instance tree.
// ─────────────────────────────────────────────────────────────────────────────
@Injectable()
export class SessionOrchestratorService implements OnDestroy {
  private sessionId = '';
  private role: 'doctor' | 'patient' = 'doctor';
  private subs: Subscription[] = [];

  // ─── Dual-channel recording setup ───────────────────────────────────────────
  // Instead of mixing both speakers into a mono track (which forces AssemblyAI
  // to *guess* who is who), we route each side onto a dedicated stereo channel:
  //   Channel 1 (Left)  → Doctor  (local microphone)
  //   Channel 2 (Right) → Patient (WebRTC remote track)
  // AssemblyAI's multichannel mode reads the physical channel index, giving us
  // 100% deterministic speaker attribution with zero labelling errors.
  // ─────────────────────────────────────────────────────────────────────────────
  private audioCtx: AudioContext | null = null;
  private mixerDest: MediaStreamAudioDestinationNode | null = null;
  private merger: ChannelMergerNode | null = null;

  // Emits the remote MediaStream the moment the first audio track arrives.
  // PatientSessionRoomComponent subscribes to this instead of polling
  // getRemoteStream() every 500 ms — no race condition, instant transition.
  readonly remoteTrackArrived$ = new Subject<MediaStream>();

  constructor(
    private signalr: SessionSignalrService,
    private webrtc: WebrtcService,
    private recorder: AudioRecorderService,
  ) {}

  async startCall(
    sessionId: string,
    role: 'doctor' | 'patient',
    accessToken: string,
  ): Promise<void> {
    this.sessionId = sessionId;
    this.role = role;

    await this.signalr.connect(sessionId, accessToken);

    const localStream = await this.webrtc.getLocalStream();

    if (role === 'doctor') {
      this.audioCtx = new AudioContext();
      if (this.audioCtx.state === 'suspended') {
        await this.audioCtx.resume();
      }

      // ── Stereo destination: 2 channels required ─────────────────────────────
      this.mixerDest = this.audioCtx.createMediaStreamDestination();
      this.mixerDest.channelCount = 2;
      this.mixerDest.channelCountMode = 'explicit';
      this.mixerDest.channelInterpretation = 'discrete';

      // ── ChannelMerger: 2 inputs → 1 stereo output ──────────────────────────
      // Input 0 → Channel 1 (Left)  = Doctor
      // Input 1 → Channel 2 (Right) = Patient
      this.merger = this.audioCtx.createChannelMerger(2);
      this.merger.connect(this.mixerDest);

      // Doctor mic → merger input 0 (Left / Channel 1)
      const localSource = this.audioCtx.createMediaStreamSource(localStream);
      localSource.connect(this.merger, 0, 0);

      this.recorder.start(this.mixerDest.stream);
    }

    // Pass remoteTrackArrived$ so WebrtcService can emit it from ontrack
    this.webrtc.createPeerConnection(sessionId, this.remoteTrackArrived$);

    if (role === 'doctor') {
      const remoteSub = this.remoteTrackArrived$.subscribe((remoteStream: MediaStream) => {
        // Patient remote audio → merger input 1 (Right / Channel 2)
        if (this.audioCtx && this.merger) {
          const remoteSource = this.audioCtx.createMediaStreamSource(remoteStream);
          remoteSource.connect(this.merger, 0, 1);
        }
      });
      this.subs.push(remoteSub);
    }

    this.registerSignalRListeners();

    if (role === 'doctor') {
      await this.signalr.notifyDoctorJoined(sessionId);

      const patientReadySub = this.signalr.patientReady$.subscribe(async (msg) => {
        const sid = (msg as any).sessionId || (msg as any).SessionId;
        if (sid?.toLowerCase() !== this.sessionId.toLowerCase()) return;
        await this.webrtc.createOffer();
      });
      this.subs.push(patientReadySub);
    }

    if (role === 'patient') {
      await this.signalr.notifyPatientReady(sessionId);

      const doctorJoinedSub = this.signalr.doctorJoined$.subscribe(async (msg) => {
        const sid = (msg as any).sessionId || (msg as any).SessionId;
        if (sid?.toLowerCase() !== this.sessionId.toLowerCase()) return;
        await this.signalr.notifyPatientReady(this.sessionId);
      });
      this.subs.push(doctorJoinedSub);
    }
  }

  async endCall(): Promise<Blob> {
    const audioBlob =
      this.role === 'doctor' ? await this.recorder.stop() : new Blob([], { type: 'audio/webm' });

    this.webrtc.close();
    await this.signalr.disconnect();
    this.cleanupSubs();
    this.closeAudioContext();

    return audioBlob;
  }

  getRecordingMimeType(): string {
    return this.recorder.mimeType;
  }

  getRemoteStream(): MediaStream | null {
    return this.webrtc.getRemoteStream();
  }

  private registerSignalRListeners(): void {
    const offerSub = this.signalr.offerReceived$.subscribe(async (msg) => {
      const sid = (msg as any).sessionId || (msg as any).SessionId;
      if (sid?.toLowerCase() !== this.sessionId.toLowerCase()) return;
      await this.webrtc.handleRemoteOfferAndAnswer(msg.sdp || (msg as any).Sdp);
    });

    const answerSub = this.signalr.answerReceived$.subscribe(async (msg) => {
      const sid = (msg as any).sessionId || (msg as any).SessionId;
      if (sid?.toLowerCase() !== this.sessionId.toLowerCase()) return;
      await this.webrtc.handleRemoteAnswer(msg.sdp || (msg as any).Sdp);
    });

    const iceSub = this.signalr.iceCandidateReceived$.subscribe(async (msg) => {
      const sid = (msg as any).sessionId || (msg as any).SessionId;
      if (sid?.toLowerCase() !== this.sessionId.toLowerCase()) return;
      await this.webrtc.addRemoteIceCandidate(msg.candidate || (msg as any).Candidate);
    });

    this.subs.push(offerSub, answerSub, iceSub);
  }

  private cleanupSubs(): void {
    this.subs.forEach((s) => s.unsubscribe());
    this.subs = [];
  }

  private closeAudioContext(): void {
    if (this.audioCtx) {
      this.audioCtx.close();
      this.audioCtx = null;
      this.mixerDest = null;
      this.merger = null;
    }
  }

  ngOnDestroy(): void {
    this.cleanupSubs();
    this.closeAudioContext();
  }
}
