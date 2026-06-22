import { Component, OnDestroy, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, interval } from 'rxjs';
import { ClinicalSessionService } from '../../services/clinical-session.service';
import { SessionOrchestratorService } from '../../services/session-orchestrator.service';
import { TranscriptService } from '../../services/transcript.service';
import { AuthService } from '../../services/auth';

type CallState =
  | 'Idle'
  | 'Connecting'
  | 'InCall'
  | 'Ending'
  | 'Uploading'
  | 'Processing'
  | 'Reviewable'
  | 'Error';

@Component({
  selector: 'app-session-room',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './session-room.component.html',
  styleUrls: ['./session-room.component.css']
})
export class SessionRoomComponent implements OnInit, AfterViewInit, OnDestroy {

  @ViewChild('remoteAudio') remoteAudioRef!: ElementRef<HTMLAudioElement>;

  state: CallState = 'Idle';
  errorMessage = '';

  bookingId = '';
  sessionId = '';

  callDurationSeconds = 0;
  private timerSub: Subscription | null = null;
  private pollSub: Subscription | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private clinicalSessionService: ClinicalSessionService,
    private orchestrator: SessionOrchestratorService,
    private transcriptService: TranscriptService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.bookingId = this.route.snapshot.paramMap.get('bookingId') ?? '';
    if (!this.bookingId) {
      this.state = 'Error';
      this.errorMessage = 'Missing booking reference. Cannot start session.';
    }
  }

  ngAfterViewInit(): void {
    // الـ <audio> element جاهز — هنربطه بالـ remote stream لما المكالمة تبدأ
  }

  get callDurationDisplay(): string {
    const minutes = Math.floor(this.callDurationSeconds / 60);
    const seconds = this.callDurationSeconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  async startCall(): Promise<void> {
    this.state = 'Connecting';
    this.errorMessage = '';

    try {
      // 1 — إنشاء الجلسة في الداتابيز
      const sessionRes = await this.clinicalSessionService
        .startSession(this.bookingId)
        .toPromise();

      this.sessionId = sessionRes!.sessionId;

      // 2 — بدء كل حاجة: SignalR + WebRTC + Recording
      const token = this.auth.getToken() ?? '';
      await this.orchestrator.startCall(this.sessionId, 'doctor', token);

      // 3 — ربط الـ remote audio بعنصر <audio> فعلياً
      const remoteStream = this.orchestrator.getRemoteStream();
      if (remoteStream && this.remoteAudioRef) {
        this.remoteAudioRef.nativeElement.srcObject = remoteStream;
      }

      this.state = 'InCall';
      this.startDurationTimer();

    } catch (err) {
      this.state = 'Error';
      this.errorMessage = this.resolveStartError(err);
    }
  }

  async endSession(): Promise<void> {
    if (this.state !== 'InCall') return;

    this.state = 'Ending';
    this.stopDurationTimer();

    try {
      // 1 — تسجيل انتهاء الجلسة في الداتابيز
      await this.clinicalSessionService.endSession(this.sessionId).toPromise();

      // 2 — قفل WebRTC + SignalR + استلام الـ Blob الصوتي
      const audioBlob = await this.orchestrator.endCall();

      // 3 — رفع الملف
      this.state = 'Uploading';
      await this.transcriptService.upload(this.sessionId, audioBlob).toPromise();

      // 4 — بدء الـ polling لحالة المعالجة
      this.state = 'Processing';
      this.startPolling();

    } catch (err) {
      this.state = 'Error';
      this.errorMessage = 'Something went wrong while ending the session. Please contact support.';
    }
  }

  private startPolling(): void {
    this.pollSub = this.transcriptService.pollStatus(this.sessionId).subscribe({
      next: (status) => {
        if (status === 'Completed') {
          this.state = 'Reviewable';
        } else if (status === 'Failed') {
          this.state = 'Error';
          this.errorMessage = 'Transcription failed. You can retry from the session details page.';
        }
      },
      error: () => {
        this.state = 'Error';
        this.errorMessage = 'Lost connection while checking transcription status.';
      }
    });
  }

  goToTranscriptReview(): void {
    this.router.navigate(['/clinical-session', this.sessionId, 'transcript']);
  }

  private startDurationTimer(): void {
    this.callDurationSeconds = 0;
    this.timerSub = interval(1000).subscribe(() => {
      this.callDurationSeconds++;
    });
  }

  private stopDurationTimer(): void {
    this.timerSub?.unsubscribe();
    this.timerSub = null;
  }

  private resolveStartError(err: any): string {
    if (err?.name === 'NotAllowedError') {
      return 'Microphone access was denied. Please allow microphone access and try again.';
    }
    if (err?.status === 400) {
      return 'This booking is not ready to start. Please check the booking status.';
    }
    return 'Could not start the session. Please try again.';
  }

  ngOnDestroy(): void {
    this.stopDurationTimer();
    this.pollSub?.unsubscribe();

    // safety net — لو الكومبوننت اتدمر والمكالمة لسه شغالة
    if (this.state === 'InCall') {
      this.orchestrator.endCall().catch(() => {});
    }
  }
}
