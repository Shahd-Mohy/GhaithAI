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

// ─── What is CallState? ───────────────────────────────────────────────────────
// This is a "union type" — the session room can only ever be in ONE of these
// states at a time. The template uses *ngIf="state === 'X'" to show/hide panels.
// The full lifecycle is:
//   Idle → Connecting → InCall → Ending → Uploading → Processing → Reviewable
// If anything goes wrong at any step, we jump straight to Error.
// ──────────────────────────────────────────────────────────────────────────────
type CallState =
  | 'Idle'        // page just loaded, waiting for doctor to click Start
  | 'Connecting'  // creating DB session + connecting SignalR + getting microphone
  | 'InCall'      // WebRTC audio call is live, MediaRecorder is capturing silently
  | 'Ending'      // doctor clicked End, we're stopping the call and saving end time
  | 'Uploading'   // audio blob is being POSTed to the backend
  | 'Processing'  // backend sent audio to AssemblyAI, we're polling every few seconds
  | 'Reviewable'  // AssemblyAI returned segments, they're saved in DB, ready to review
  | 'Error';      // something went wrong — errorMessage has the details

@Component({
  selector: 'app-session-room',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './session-room.component.html',
  styleUrls: ['./session-room.component.css']
})
export class SessionRoomComponent implements OnInit, AfterViewInit, OnDestroy {

  // ── @ViewChild: gives us a direct reference to the <audio #remoteAudio> element
  // in the HTML so we can attach the remote WebRTC stream to it programmatically.
  @ViewChild('remoteAudio') remoteAudioRef!: ElementRef<HTMLAudioElement>;

  state: CallState = 'Idle';
  errorMessage = '';

  // ── errorDetail ────────────────────────────────────────────────────────────
  // NEW. Holds the specific reason the backend/speech-vendor gave for a
  // transcription failure (e.g. "Unsupported audio codec"), when available.
  // errorMessage stays as the human-friendly headline; errorDetail is the
  // technical line shown underneath for debugging/support purposes. It's
  // empty whenever the backend doesn't provide one — the UI should hide
  // that line in that case, not show "undefined".
  errorDetail = '';

  // bookingId comes from the parent dashboard via [bookingId]="selectedBookingId"
  // It identifies which appointment this session belongs to.
  @Input() bookingId = '';

  // sessionId is returned by the backend when we call startSession().
  // It's the GUID of the ClinicalSession row created in the DB.
  // We store it here and use it for every subsequent API call in this flow.
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

    // ── WHY NgZone? ────────────────────────────────────────────────────────────
    // Angular tracks async work using a system called "Zone.js".
    // HTTP calls made through HttpClient are tracked — Angular knows when they
    // finish and automatically updates the screen.
    // BUT: WebRTC (getUserMedia, createOffer) and SignalR are native browser APIs
    // that run OUTSIDE Angular's zone. Angular doesn't know when they finish,
    // so it never re-renders the template even though our state variable changed.
    // This was the exact reason the UI appeared "frozen" until you opened DevTools
    // (DevTools triggers a repaint that accidentally woke Angular up).
    // ngZone.run(() => { ... }) tells Angular: "run this code inside my zone,
    // track it, and update the screen when it's done."
    // ──────────────────────────────────────────────────────────────────────────
    private ngZone: NgZone,

    // ── WHY ChangeDetectorRef? ──────────────────────────────────────────────
    // Even inside ngZone.run(), after an await, Angular sometimes doesn't
    // re-check the component automatically. cdr.detectChanges() is an explicit
    // "please look at this component right now and update the DOM if anything
    // changed." We call it after every state change to guarantee the user
    // sees the correct panel immediately.
    // ──────────────────────────────────────────────────────────────────────────
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // bookingId can arrive either as an @Input() from the parent component
    // OR as a route parameter (:bookingId) if the session room is a separate route.
    // We check @Input() first, then fall back to the route.
    if (!this.bookingId) {
      this.bookingId = this.route.snapshot.paramMap.get('bookingId') ?? '';
    }

    // If after both checks we still have no bookingId, something is wrong upstream.
    // Show an error immediately — no point proceeding without a booking reference.
    if (!this.bookingId) {
      this.state = 'Error';
      this.errorMessage = 'Missing booking reference. Cannot start session.';
    }
  }

  ngAfterViewInit(): void {}

  // Formats seconds into MM:SS for the in-call duration display.
  get callDurationDisplay(): string {
    const minutes = Math.floor(this.callDurationSeconds / 60);
    const seconds = this.callDurationSeconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  // ── STEP 1: Doctor clicks "Start Call" ───────────────────────────────────────
  // Everything is wrapped in ngZone.run() so Angular tracks the async work
  // and updates the screen at each state change.
  async startCall(): Promise<void> {
    this.ngZone.run(async () => {
      this.state = 'Connecting';
      this.errorMessage = '';
      this.errorDetail = '';
      this.cdr.detectChanges(); // show the Connecting spinner immediately

      try {
        // ── 1a. Create the ClinicalSession record in the database ──────────────
        // POST /api/sessions/start → returns { id: "guid", message: "..." }
        // lastValueFrom() is the modern replacement for .toPromise().
        // It converts an Observable into a Promise that resolves with the LAST value.
        // We use it here so we can await the HTTP call inside an async function.
        const sessionRes = await lastValueFrom(
          this.clinicalSessionService.startSession(this.bookingId)
        );

        // Store the session GUID — every call from here on needs it.
        this.sessionId = sessionRes!.id;

        // ── 1b. Connect SignalR + get microphone + start WebRTC + start recording ─
        // The orchestrator handles all of this in the right order.
        // See session-orchestrator.service.ts for the full sequence.
        const token = this.auth.getToken() ?? '';
        await this.orchestrator.startCall(this.sessionId, 'doctor', token);

        // ── 1c. Attach the remote patient audio stream to the <audio> element ───
        // Without this line, the doctor can't hear the patient.
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

  // ── STEP 2: Doctor clicks "End Session" ──────────────────────────────────────
  async endSession(): Promise<void> {
    // Guard: only allow ending if we're actually in a call.
    if (this.state !== 'InCall') return;

    this.ngZone.run(async () => {
      this.state = 'Ending';
      this.stopDurationTimer();
      this.cdr.detectChanges();

      try {
        // ── 2a. Record the session end time in the database ───────────────────
        // PUT /api/sessions/{sessionId}/end
        await lastValueFrom(this.clinicalSessionService.endSession(this.sessionId));

        // ── 2b. Stop everything: WebRTC closes, SignalR disconnects,
        //        MediaRecorder stops and assembles all recorded chunks into one Blob.
        const audioBlob = await this.orchestrator.endCall();

        // ── 2c. Upload the audio Blob to the backend ──────────────────────────
        // POST /api/sessions/{sessionId}/transcript/upload (multipart/form-data)
        // Backend saves it to audio-temp/, sets TranscriptionStatus = Processing,
        // then fires Task.Run(() => transcriptionService.ProcessAsync(...))
        // which uploads to AssemblyAI and polls until done — all in the background.
        //
        // NOTE: audioBlob.type now carries the *actual* codec-qualified MIME
        // type the recorder used (see audio-recorder.service.ts). upload()
        // reads that to pick the right file extension — don't override it here.
        this.state = 'Uploading';
        this.cdr.detectChanges();
        await lastValueFrom(this.transcriptService.upload(this.sessionId, audioBlob));

        // ── 2d. Start polling the backend every few seconds ───────────────────
        // GET /api/sessions/{sessionId}/transcript/status
        // Returns { status: "Processing" | "Completed" | "Failed", errorDetail? }
        // The polling stops automatically when status is no longer "Processing".
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

  // ── STEP 3: Poll until AssemblyAI finishes transcribing ──────────────────────
  // pollStatus() is an Observable that keeps calling the status endpoint
  // with exponential backoff (5s → 7s → 10s → 15s max) until it gets
  // "Completed" or "Failed", then completes automatically.
  //
  // CHANGED: pollStatus() now emits the full TranscriptStatusResponse object
  // (not just the status string), so we can read res.errorDetail and show the
  // doctor the *actual* reason transcription failed instead of only the
  // generic "Transcription failed" line.
  private startPolling(): void {
    this.pollSub = this.transcriptService.pollStatus(this.sessionId).subscribe({
      next: (res) => {
        // RxJS subscriptions can run outside Angular's zone, so we wrap
        // every state change in ngZone.run() to guarantee a UI update.
        this.ngZone.run(() => {
          if (res.status === 'Completed') {
            // Transcription succeeded — show the "Review Transcript" button.
            this.state = 'Reviewable';
            this.cdr.detectChanges();
          } else if (res.status === 'Failed') {
            // AssemblyAI returned an error or the backend crashed during processing.
            this.state = 'Error';
            this.errorMessage = 'Transcription failed. You can retry from the session details page.';
            // Only set errorDetail if the backend actually sent one — otherwise
            // leave it blank so the template can hide that line entirely.
            this.errorDetail = res.errorDetail ?? '';
            this.cdr.detectChanges();
          }
          // If status is 'Processing' or 'NotStarted', we just wait for the
          // next poll — the Observable handles the timing automatically.
        });
      },
      error: () => {
        // This fires if the HTTP request itself fails (network error, 404, 500).
        // The most common cause would be the sessionId being wrong or the backend
        // being unreachable during polling.
        this.ngZone.run(() => {
          this.state = 'Error';
          this.errorMessage = 'Lost connection while checking transcription status.';
          this.errorDetail = '';
          this.cdr.detectChanges();
        });
      }
    });
  }

  // ── STEP 4: Doctor clicks "Review Transcript" ────────────────────────────────
  // Navigate to the transcript viewer route.
  goToTranscriptReview(): void {
    this.router.navigate(['/clinical-session', this.sessionId, 'transcript']);
  }

  // ── Timer helpers ─────────────────────────────────────────────────────────────
  private startDurationTimer(): void {
    this.callDurationSeconds = 0;
    // interval(1000) emits a number every second.
    // We wrap the increment in ngZone.run() so the displayed time actually updates.
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

  // Maps raw error objects to human-readable messages for the doctor.
  private resolveStartError(err: any): string {
    if (err?.name === 'NotAllowedError') {
      return 'Microphone access was denied. Please allow microphone access and try again.';
    }
    if (err?.status === 400) {
      return 'This booking is not ready to start. Please check the booking status.';
    }
    if (err?.message === 'No supported audio recording MIME type found in this browser.') {
      return 'This browser cannot record audio in a supported format. Please try Chrome, Edge, or Firefox.';
    }
    return 'Could not start the session. Please try again.';
  }

  ngOnDestroy(): void {
    this.stopDurationTimer();
    this.pollSub?.unsubscribe();

    // Safety net: if the component is destroyed while a call is in progress
    // (e.g. doctor navigates away accidentally), clean up WebRTC and SignalR.
    if (this.state === 'InCall') {
      this.orchestrator.endCall().catch(() => {});
    }
  }
}