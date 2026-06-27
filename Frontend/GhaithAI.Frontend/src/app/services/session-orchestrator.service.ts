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

  // Audio mixer: combines local mic + remote (patient) audio into one stream
  // so both sides of the conversation are captured in the recording.
  private audioCtx: AudioContext | null = null;
  private mixerDest: MediaStreamAudioDestinationNode | null = null;

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
      this.mixerDest = this.audioCtx.createMediaStreamDestination();

      const localSource = this.audioCtx.createMediaStreamSource(localStream);
      localSource.connect(this.mixerDest);

      this.recorder.start(this.mixerDest.stream);
    }

    // Pass remoteTrackArrived$ so WebrtcService can emit it from ontrack
    this.webrtc.createPeerConnection(sessionId, this.remoteTrackArrived$);

    if (role === 'doctor') {
      const remoteSub = this.remoteTrackArrived$.subscribe((remoteStream: MediaStream) => {
        if (this.audioCtx && this.mixerDest) {
          const remoteSource = this.audioCtx.createMediaStreamSource(remoteStream);
          remoteSource.connect(this.mixerDest);
        }
      });
      this.subs.push(remoteSub);
    }

    this.registerSignalRListeners();

    if (role === 'doctor') {
      await this.webrtc.createOffer();
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
      if (msg.sessionId !== this.sessionId) return;
      await this.webrtc.handleRemoteOfferAndAnswer(msg.sdp);
    });

    const answerSub = this.signalr.answerReceived$.subscribe(async (msg) => {
      if (msg.sessionId !== this.sessionId) return;
      await this.webrtc.handleRemoteAnswer(msg.sdp);
    });

    const iceSub = this.signalr.iceCandidateReceived$.subscribe(async (msg) => {
      if (msg.sessionId !== this.sessionId) return;
      await this.webrtc.addRemoteIceCandidate(msg.candidate);
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
    }
  }

  ngOnDestroy(): void {
    this.cleanupSubs();
    this.closeAudioContext();
  }
}
