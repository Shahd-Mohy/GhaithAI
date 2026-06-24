import { Injectable, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { SessionSignalrService } from './session-signalr.service';
import { WebrtcService } from './webrtc.service';
import { AudioRecorderService } from './audio-recorder.service';

@Injectable({ providedIn: 'root' })
export class SessionOrchestratorService implements OnDestroy {

  private sessionId = '';
  private role: 'doctor' | 'patient' = 'doctor';
  private subs: Subscription[] = [];

  constructor(
    private signalr: SessionSignalrService,
    private webrtc: WebrtcService,
    private recorder: AudioRecorderService
  ) {}

  // ─── Main Entry Point ─────────────────────────────
  async startCall(
    sessionId: string,
    role: 'doctor' | 'patient',
    accessToken: string
  ): Promise<void> {

    this.sessionId = sessionId;
    this.role = role;

    // 1 ─ اتصل بالـ SignalR hub
    await this.signalr.connect(sessionId, accessToken);

    // 2 ─ خد الـ microphone
    const localStream = await this.webrtc.getLocalStream();

    // 3 ─ ابدأ التسجيل الصامت
    //     Can throw (no supported MIME type) — let it propagate to the
    //     component's catch block so the doctor sees a real error instead
    //     of silently recording nothing.
    this.recorder.start(localStream);

    // 4 ─ عمل الـ RTCPeerConnection
    this.webrtc.createPeerConnection(sessionId);

    // 5 ─ سجّل الـ SignalR listeners
    this.registerSignalRListeners();

    // 6 ─ لو doctor: ابعت offer
    //     لو patient: استنى offer يجي
    if (role === 'doctor') {
      await this.webrtc.createOffer();
    }
    // Patient مش محتاج يعمل حاجة - هيستنى ReceiveOffer event
  }

  // ─── End Call ─────────────────────────────────────
  async endCall(): Promise<Blob> {

    // 1 ─ وقف التسجيل واستلم الـ blob
    const audioBlob = await this.recorder.stop();

    // 2 ─ قفل WebRTC
    this.webrtc.close();

    // 3 ─ قطع SignalR
    await this.signalr.disconnect();

    // 4 ─ إلغاء الـ subscriptions
    this.cleanupSubs();

    return audioBlob;
  }

  // ─── Get the exact MIME type (with codec) the recorder used ──────────────
  // The session-room component needs this to build the correct filename/
  // extension for upload — never hardcode '.webm' separately, or the
  // filename and the actual encoded bytes can disagree (this was the root
  // cause of transcription silently failing).
  getRecordingMimeType(): string {
    return this.recorder.mimeType;
  }

  // ─── Get Remote Stream للـ <audio> element ────────
  getRemoteStream(): MediaStream | null {
    return this.webrtc.getRemoteStream();
  }

  // ─── SignalR Listeners ────────────────────────────
  private registerSignalRListeners(): void {

    // لما يوصل offer (patient side)
    const offerSub = this.signalr.offerReceived$.subscribe(async (msg) => {
      if (msg.sessionId !== this.sessionId) return;
      await this.webrtc.handleRemoteOfferAndAnswer(msg.sdp);
    });

    // لما يوصل answer (doctor side)
    const answerSub = this.signalr.answerReceived$.subscribe(async (msg) => {
      if (msg.sessionId !== this.sessionId) return;
      await this.webrtc.handleRemoteAnswer(msg.sdp);
    });

    // لما يوصل ICE candidate (الاتنين)
    const iceSub = this.signalr.iceCandidateReceived$.subscribe(async (msg) => {
      if (msg.sessionId !== this.sessionId) return;
      await this.webrtc.addRemoteIceCandidate(msg.candidate);
    });

    this.subs.push(offerSub, answerSub, iceSub);
  }

  // ─── Cleanup ──────────────────────────────────────
  private cleanupSubs(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.subs = [];
  }

  ngOnDestroy(): void {
    this.cleanupSubs();
  }
}