import {
  Component, OnDestroy, OnInit,
  ViewChild, ElementRef, AfterViewInit,
  Input, NgZone, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, interval } from 'rxjs';
import { lastValueFrom } from 'rxjs';
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

  @Input() bookingId = '';
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
    private auth: AuthService,
    private ngZone: NgZone,           // ← ADDED: tells Angular when async work finishes
    private cdr: ChangeDetectorRef    // ← ADDED: forces view to re-render after state changes
  ) {}

  ngOnInit(): void {
    if (!this.bookingId) {
      this.bookingId = this.route.snapshot.paramMap.get('bookingId') ?? '';
    }
    if (!this.bookingId) {
      this.state = 'Error';
      this.errorMessage = 'Missing booking reference. Cannot start session.';
    }
  }

  ngAfterViewInit(): void {}

  get callDurationDisplay(): string {
    const minutes = Math.floor(this.callDurationSeconds / 60);
    const seconds = this.callDurationSeconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  // WHY ngZone.run() here?
  // WebRTC calls (getUserMedia, createOffer) and SignalR are native browser APIs
  // that run OUTSIDE Angular's zone. When they resolve, Angular doesn't know
  // the async work finished, so it never re-renders the template.
  // ngZone.run() tells Angular: "this code is happening, please track it."
  // This is exactly why it looked like everything was frozen until you opened
  // DevTools — DevTools triggers a repaint cycle which accidentally fixed it.
  async startCall(): Promise<void> {
    this.ngZone.run(async () => {
      this.state = 'Connecting';
      this.errorMessage = '';
      this.cdr.detectChanges();

      try {
        // 1 — Create the session record in the database
        // lastValueFrom is the modern replacement for .toPromise()
        const sessionRes = await lastValueFrom(
          this.clinicalSessionService.startSession(this.bookingId)
        );

        this.sessionId = sessionRes!.id;

        // 2 — Start SignalR + WebRTC + Recording
        const token = this.auth.getToken() ?? '';
        await this.orchestrator.startCall(this.sessionId, 'doctor', token);

        // 3 — Connect remote audio stream to the <audio> element
        const remoteStream = this.orchestrator.getRemoteStream();
        if (remoteStream && this.remoteAudioRef) {
          this.remoteAudioRef.nativeElement.srcObject = remoteStream;
        }

        this.state = 'InCall';
        this.startDurationTimer();
        this.cdr.detectChanges();

      } catch (err) {
        this.state = 'Error';
        this.errorMessage = this.resolveStartError(err);
        this.cdr.detectChanges();
      }
    });
  }

  async endSession(): Promise<void> {
    if (this.state !== 'InCall') return;

    this.ngZone.run(async () => {
      this.state = 'Ending';
      this.stopDurationTimer();
      this.cdr.detectChanges();

      try {
        // 1 — Record session end time in the database
        await lastValueFrom(this.clinicalSessionService.endSession(this.sessionId));

        // 2 — Stop WebRTC + SignalR + get the recorded audio blob
        const audioBlob = await this.orchestrator.endCall();

        // 3 — Upload the audio file to backend
        this.state = 'Uploading';
        this.cdr.detectChanges();
        await lastValueFrom(this.transcriptService.upload(this.sessionId, audioBlob));

        // 4 — Start polling for transcription status
        this.state = 'Processing';
        this.cdr.detectChanges();
        this.startPolling();

      } catch (err) {
        this.state = 'Error';
        this.errorMessage = 'Something went wrong while ending the session. Please contact support.';
        this.cdr.detectChanges();
      }
    });
  }

  private startPolling(): void {
    this.pollSub = this.transcriptService.pollStatus(this.sessionId).subscribe({
      next: (status) => {
        // This runs inside an RxJS subscription which may be outside Angular's zone.
        // We wrap state changes in ngZone.run() to ensure the template updates.
        this.ngZone.run(() => {
          if (status === 'Completed') {
            this.state = 'Reviewable';
            this.cdr.detectChanges();
          } else if (status === 'Failed') {
            this.state = 'Error';
            this.errorMessage = 'Transcription failed. You can retry from the session details page.';
            this.cdr.detectChanges();
          }
        });
      },
      error: () => {
        this.ngZone.run(() => {
          this.state = 'Error';
          this.errorMessage = 'Lost connection while checking transcription status.';
          this.cdr.detectChanges();
        });
      }
    });
  }

  goToTranscriptReview(): void {
    this.router.navigate(['/clinical-session', this.sessionId, 'transcript']);
  }

  private startDurationTimer(): void {
    this.callDurationSeconds = 0;
    this.timerSub = interval(1000).subscribe(() => {
      this.ngZone.run(() => {
        this.callDurationSeconds++;
      });
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

    if (this.state === 'InCall') {
      this.orchestrator.endCall().catch(() => {});
    }
  }
}