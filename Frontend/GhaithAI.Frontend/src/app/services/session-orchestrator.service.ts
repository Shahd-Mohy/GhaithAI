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

  // Emits the remote MediaStream the moment the first audio track arrives.
  // PatientSessionRoomComponent subscribes to this instead of polling
  // getRemoteStream() every 500 ms — no race condition, instant transition.
  readonly remoteTrackArrived$ = new Subject<MediaStream>();

  constructor(
    private signalr: SessionSignalrService,
    private webrtc: WebrtcService,
    private recorder: AudioRecorderService
  ) { }

  async startCall(
    sessionId: string,
    role: 'doctor' | 'patient',
    accessToken: string
  ): Promise<void> {
    this.sessionId = sessionId;
    this.role = role;

    await this.signalr.connect(sessionId, accessToken);

    const localStream = await this.webrtc.getLocalStream();

    if (role === 'doctor') {
      this.recorder.start(localStream);
    }

    // Pass remoteTrackArrived$ so WebrtcService can emit it from ontrack
    this.webrtc.createPeerConnection(sessionId, this.remoteTrackArrived$);

    this.registerSignalRListeners();

    if (role === 'doctor') {
      await this.webrtc.createOffer();
    } else {
      await this.signalr.notifyPatientReady(sessionId);
    }
  }

  async endCall(): Promise<Blob> {
    const audioBlob = this.role === 'doctor'
      ? await this.recorder.stop()
      : new Blob([], { type: 'audio/webm' });

    this.webrtc.close();
    await this.signalr.disconnect();
    this.cleanupSubs();

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

    const patientReadySub = this.signalr.patientReady$.subscribe(async (msg) => {
      if (this.role !== 'doctor') return;
      if (msg.sessionId !== this.sessionId) return;
      await this.webrtc.createOffer();
    });

    this.subs.push(offerSub, answerSub, iceSub, patientReadySub);
  }

  private cleanupSubs(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.subs = [];
  }

  ngOnDestroy(): void {
    this.cleanupSubs();
  }
}
