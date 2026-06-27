import {
  Component, OnDestroy, OnInit,
  ViewChild, ElementRef, Input,
  NgZone, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, interval } from 'rxjs';
import { lastValueFrom } from 'rxjs';
import { ClinicalSessionService } from '../../services/clinical-session.service';
import { DoctorService, UserBooking } from '../../services/doctor.service';
import { SessionOrchestratorService } from '../../services/session-orchestrator.service';
import { SessionSignalrService } from '../../services/session-signalr.service';
import { WebrtcService } from '../../services/webrtc.service';
import { AudioRecorderService } from '../../services/audio-recorder.service';
import { AuthService } from '../../services/auth';

type CallState =
  | 'LoadingSession'
  | 'NoSession'
  | 'Idle'
  | 'Connecting'
  | 'Waiting'
  | 'InCall'
  | 'Ending'
  | 'Ended'
  | 'Error';

@Component({
  selector: 'app-patient-session-room',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './patient-session-room.component.html',
  styleUrls: ['./patient-session-room.component.css'],
  providers: [
    SessionOrchestratorService,
    SessionSignalrService,
    WebrtcService,
    AudioRecorderService,
  ]
})
export class PatientSessionRoomComponent implements OnInit, OnDestroy {

  @ViewChild('remoteAudio') remoteAudioRef!: ElementRef<HTMLAudioElement>;

  state: CallState = 'LoadingSession';
  errorMessage = '';

  @Input() sessionId = '';

  doctorName = '';
  bookingDate = '';

  /** Counts down polling attempts before giving up */
  private pollAttempt = 0;
  private readonly MAX_POLL_ATTEMPTS = 0; // 0 = poll forever until session found or user navigates away

  get doctorInitials(): string {
    return this.doctorName
      .split(' ')
      .filter(Boolean)
      .map(w => w[0])
      .join('')
      .substring(0, 2)
      .toUpperCase() || 'DR';
  }

  callDurationSeconds = 0;
  private timerSub: Subscription | null = null;
  private trackSub: Subscription | null = null;
  private pollSub: Subscription | null = null;

  /** Bookings fetched once, reused across poll cycles */
  private cachedBookings: UserBooking[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private clinicalSessionService: ClinicalSessionService,
    private doctorService: DoctorService,
    private orchestrator: SessionOrchestratorService,
    private auth: AuthService,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) { }

  get stateLabel(): string {
    const map: Record<CallState, string> = {
      LoadingSession: 'Searching…',
      NoSession: 'Waiting for Doctor',
      Idle: 'Ready to Join',
      Connecting: 'Connecting…',
      Waiting: 'Waiting for Doctor…',
      InCall: 'In Call',
      Ending: 'Leaving…',
      Ended: 'Call Ended',
      Error: 'Error'
    };
    return map[this.state] ?? this.state;
  }

  get statusSubLabel(): string {
    switch (this.state) {
      case 'LoadingSession': return 'Looking up your appointment…';
      case 'NoSession': return 'Your doctor hasn\'t started the session yet. Checking automatically…';
      case 'Idle': return 'Your doctor has started the session. Tap Join Call to connect.';
      case 'Connecting': return 'Setting up your microphone…';
      case 'Waiting': return 'You\'re connected — the doctor will join shortly';
      case 'InCall': return 'Connected with your doctor';
      case 'Ending': return 'Leaving the call…';
      case 'Ended': return 'You\'ve left the call';
      default: return '';
    }
  }

  get callDurationDisplay(): string {
    const m = Math.floor(this.callDurationSeconds / 60);
    const s = this.callDurationSeconds % 60;
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
  }

  ngOnInit(): void {
    // Priority 1: explicit sessionId input from parent
    if (this.sessionId) {
      this.startAutoJoinIfReady(this.sessionId);
      return;
    }

    // Priority 2: sessionId in route params
    const routeSessionId = this.route.snapshot.paramMap.get('sessionId') ?? '';
    if (routeSessionId) {
      this.sessionId = routeSessionId;
      this.startAutoJoinIfReady(this.sessionId);
      return;
    }

    // Priority 3: discover from bookings, then auto-poll every 8s
    this.runDiscovery();
    this.startPollingForSession();
  }

  // ─── Discovery ─────────────────────────────────────────────────────────────

  /**
   * Core discovery: fetch bookings → for each, call POST /sessions/start.
   * POST /sessions/start is the SAME API the doctor uses. If a session already
   * exists for that bookingId the backend returns the existing sessionId
   * (idempotent). If not yet started, it errors → we try the next booking.
   * 
   * This is the correct "join the same room" pattern used by both sides.
   */
  private async runDiscovery(): Promise<void> {
    // Only show spinner on first attempt
    if (this.state !== 'NoSession') {
      this.state = 'LoadingSession';
      this.cdr.detectChanges();
    }

    try {
      // Fetch bookings only once, cache for subsequent poll cycles
      if (this.cachedBookings.length === 0) {
        const bookings = await lastValueFrom(
          this.doctorService.getMyBookings('upcoming', 0, 50)
        );
        // Keep all non-cancelled bookings
        this.cachedBookings = (bookings ?? []).filter(b => {
          const s = (b.status ?? '').toLowerCase();
          return s !== 'cancelled' && s !== 'canceled';
        });
      }

      if (this.cachedBookings.length === 0) {
        this.ngZone.run(() => { this.state = 'NoSession'; this.cdr.detectChanges(); });
        return;
      }

      // Probe each booking using GET /sessions/by-booking/{bookingId}
      // which is the lightweight read-only lookup (no side effects).
      for (const booking of this.cachedBookings) {
        try {
          const session = await lastValueFrom(
            this.clinicalSessionService.getSessionByBookingId(booking.bookingId)
          );

          if (session && session.id) {
            const status = (session.status ?? '').toLowerCase();
            const isJoinable = status !== 'completed' && status !== 'ended' && status !== 'cancelled';

            if (isJoinable) {
              this.ngZone.run(() => {
                this.sessionId = session.id;
                this.doctorName = booking.doctorName || session.doctorName || 'Your Doctor';
                this.bookingDate = booking.displayTime || booking.bookingDate;
                this.state = 'Idle';
                this.stopPollingForSession(); // found — no more polling needed
                this.cdr.detectChanges();
              });
              return;
            }
          }
        } catch {
          // 404 = session not started for this booking yet — try next
          continue;
        }
      }

      // Nothing found this cycle — stay in NoSession (polling will retry)
      this.ngZone.run(() => {
        this.state = 'NoSession';
        this.cdr.detectChanges();
      });

    } catch {
      this.ngZone.run(() => { this.state = 'NoSession'; this.cdr.detectChanges(); });
    }
  }

  /**
   * Auto-polls every 8 seconds — like waiting for an incoming call.
   * Stops automatically once a joinable session is discovered.
   */
  private startPollingForSession(): void {
    if (this.pollSub) return; // already polling
    this.pollSub = interval(8000).subscribe(() => {
      // Don't poll if already in an active call state
      if (this.state === 'Idle' || this.state === 'Connecting' ||
        this.state === 'Waiting' || this.state === 'InCall' ||
        this.state === 'Ending' || this.state === 'Ended') {
        return;
      }
      this.runDiscovery();
    });
  }

  private stopPollingForSession(): void {
    this.pollSub?.unsubscribe();
    this.pollSub = null;
  }

  /** Called when sessionId is already known — just verify it's still active. */
  private startAutoJoinIfReady(sessionId: string): void {
    this.state = 'LoadingSession';
    this.cdr.detectChanges();

    this.clinicalSessionService.getSession(sessionId).subscribe({
      next: (session) => {
        this.ngZone.run(() => {
          if (!session) {
            this.state = 'NoSession';
          } else {
            const status = (session.status ?? '').toLowerCase();
            if (status === 'completed' || status === 'ended' || status === 'cancelled') {
              this.state = 'NoSession';
            } else {
              this.doctorName = session.doctorName || 'Your Doctor';
              this.state = 'Idle';
            }
          }
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          this.state = err?.status === 404 ? 'NoSession' : 'Error';
          if (this.state === 'Error') {
            this.errorMessage = 'Could not load session. Please try again.';
          }
          this.cdr.detectChanges();
        });
      }
    });
  }

  // ─── Call control ──────────────────────────────────────────────────────────

  async joinCall(): Promise<void> {
    if (this.state !== 'Idle') return;

    this.ngZone.run(async () => {
      this.state = 'Connecting';
      this.errorMessage = '';
      this.cdr.detectChanges();

      try {
        const token = this.auth.getToken() ?? '';

        // role='patient' → orchestrator skips recorder.start() and createOffer()
        await this.orchestrator.startCall(this.sessionId, 'patient', token);

        this.trackSub = this.orchestrator.remoteTrackArrived$.subscribe((stream: MediaStream) => {
          this.ngZone.run(() => {
            this.state = 'InCall';
            this.startDurationTimer();
            setTimeout(() => {
              if (this.remoteAudioRef?.nativeElement) {
                this.remoteAudioRef.nativeElement.srcObject = stream;
              }
            }, 0);
            this.cdr.detectChanges();
          });
        });

        this.state = 'Waiting';
        this.cdr.detectChanges();

      } catch (err) {
        this.ngZone.run(() => {
          this.state = 'Error';
          this.errorMessage = this.resolveJoinError(err);
          this.cdr.detectChanges();
        });
      }
    });
  }

  async leaveCall(): Promise<void> {
    if (this.state !== 'InCall' && this.state !== 'Waiting') return;

    this.ngZone.run(async () => {
      this.state = 'Ending';
      this.stopDurationTimer();
      this.trackSub?.unsubscribe();
      this.trackSub = null;
      this.cdr.detectChanges();

      try {
        await this.orchestrator.endCall();
        this.state = 'Ended';
        this.cdr.detectChanges();
      } catch {
        this.state = 'Error';
        this.errorMessage = 'Something went wrong while leaving the call.';
        this.cdr.detectChanges();
      }
    });
  }

  retry(): void {
    this.sessionId = '';
    this.doctorName = '';
    this.bookingDate = '';
    this.cachedBookings = []; // force re-fetch on next discovery
    this.stopPollingForSession();
    this.ngOnInit();
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  // ─── Helpers ───────────────────────────────────────────────────────────────

  private startDurationTimer(): void {
    this.callDurationSeconds = 0;
    this.timerSub = interval(1000).subscribe(() => {
      this.ngZone.run(() => {
        this.callDurationSeconds++;
        this.cdr.detectChanges();
      });
    });
  }

  private stopDurationTimer(): void {
    this.timerSub?.unsubscribe();
    this.timerSub = null;
  }

  private resolveJoinError(err: any): string {
    if (err?.name === 'NotAllowedError') {
      return 'Microphone access was denied. Please allow microphone access and try again.';
    }
    if (err?.status === 404) {
      return 'This session could not be found. The doctor may not have started it yet.';
    }
    if (err?.status === 403) {
      return 'You are not authorised to join this session.';
    }
    return 'Could not join the call. Please try again.';
  }

  ngOnDestroy(): void {
    this.stopDurationTimer();
    this.stopPollingForSession();
    this.trackSub?.unsubscribe();

    if (this.state === 'InCall' || this.state === 'Waiting') {
      this.orchestrator.endCall().catch(() => { });
    }
  }
}