import {
  Component, OnDestroy, OnInit,
  ViewChild, ElementRef, AfterViewInit,
  Input, NgZone, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, interval } from 'rxjs';
import { lastValueFrom } from 'rxjs';
import { ClinicalSessionService, SessionNote } from '../../services/clinical-session.service';
import { SessionOrchestratorService } from '../../services/session-orchestrator.service';
import { SessionSignalrService } from '../../services/session-signalr.service';
import { WebrtcService } from '../../services/webrtc.service';
import { AudioRecorderService } from '../../services/audio-recorder.service';
import { TranscriptService } from '../../services/transcript.service';
import { AuthService } from '../../services/auth';

type CallState =
  | 'Idle'
  | 'Connecting'
  | 'InCall'
  | 'Ending'
  | 'Uploading'
  | 'Processing'
  | 'Error';

@Component({
  selector: 'app-session-room',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './session-room.component.html',
  styleUrls: ['./session-room.component.css'],

  // ─── WHY providers here? ─────────────────────────────────────────────────────
  // Mirrors the same list in PatientSessionRoomComponent. Each component gets
  // its own isolated instance of the orchestrator and its dependencies — the
  // doctor's WebRTC peer connection and SignalR hub can never collide with the
  // patient's, even if both components are active at the same time.
  // ─────────────────────────────────────────────────────────────────────────────
  providers: [
    SessionOrchestratorService,
    SessionSignalrService,
    WebrtcService,
    AudioRecorderService,
  ]
})
export class SessionRoomComponent implements OnInit, AfterViewInit, OnDestroy {

  @ViewChild('remoteAudio') remoteAudioRef!: ElementRef<HTMLAudioElement>;

  state: CallState = 'Idle';
  errorMessage = '';
  errorDetail = '';

  @Input() bookingId = '';
  @Input() patientName = 'Patient';
  @Input() patientAge = '';
  @Input() patientCondition = '';
  @Input() prevSessionInfo = '';

  get patientInitials(): string {
    return this.patientName
      .split(' ')
      .filter(Boolean)
      .map(w => w[0])
      .join('')
      .substring(0, 2)
      .toUpperCase() || 'PT';
  }

  sessionId = '';
  sessionNotes = '';
  notes: SessionNote[] = [];
  isSavingNote = false;
  notesError = '';
  editingNoteId: string | null = null;
  editingContent = '';
  isUpdatingNote = false;

  callDurationSeconds = 0;
  private timerSub: Subscription | null = null;
  private pollSub: Subscription | null = null;

  get stateLabel(): string {
    const map: Record<CallState, string> = {
      Idle: 'Ready', Connecting: 'Connecting…', InCall: 'In Call',
      Ending: 'Ending…', Uploading: 'Uploading…', Processing: 'Processing…',
      Error: 'Error'
    };
    return map[this.state] ?? this.state;
  }

  get timerStatusLabel(): string {
    if (this.state === 'Idle') return 'Ready to record';
    if (this.state === 'InCall') return 'Recording in progress';
    if (this.state === 'Connecting') return 'Connecting…';
    if (this.state === 'Ending') return 'Closing session…';
    if (this.state === 'Uploading') return 'Uploading…';
    if (this.state === 'Processing') return 'Transcribing…';
    return '';
  }

  get callDurationDisplay(): string {
    const m = Math.floor(this.callDurationSeconds / 60);
    const s = this.callDurationSeconds % 60;
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  }

  handleCancel(): void {
    if (this.state === 'InCall') this.endSession();
    else this.router.navigate(['/clinician-dashboard']);
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private clinicalSessionService: ClinicalSessionService,
    private orchestrator: SessionOrchestratorService,
    private transcriptService: TranscriptService,
    private auth: AuthService,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    if (!this.bookingId) {
      this.bookingId = this.route.snapshot.paramMap.get('bookingId') ?? '';
    }
    if (!this.bookingId) {
      this.state = 'Error';
      this.errorMessage = 'Missing booking reference. Cannot start session.';
    }
  }

  ngAfterViewInit(): void { }

  async startCall(): Promise<void> {
    this.ngZone.run(async () => {
      this.state = 'Connecting';
      this.errorMessage = '';
      this.errorDetail = '';
      this.cdr.detectChanges();

      try {
        const sessionRes = await lastValueFrom(
          this.clinicalSessionService.startSession(this.bookingId)
        );
        this.sessionId = sessionRes!.id;
        this.loadNotes();

        const token = this.auth.getToken() ?? '';
        await this.orchestrator.startCall(this.sessionId, 'doctor', token);

        // Wire up remote audio immediately (if patient already joined)
        const remoteStream = this.orchestrator.getRemoteStream();
        if (remoteStream && this.remoteAudioRef) {
          console.log('[Doctor Component] Wire up remote audio immediately:', remoteStream);
          this.remoteAudioRef.nativeElement.srcObject = remoteStream;
          this.remoteAudioRef.nativeElement.play().catch(err => console.error('[Doctor Component] Play remote audio failed:', err));
        }

        // Also subscribe for when patient joins AFTER the doctor is in InCall
        this.orchestrator.remoteTrackArrived$.subscribe((stream: MediaStream) => {
          console.log('[Doctor Component] remoteTrackArrived$ emitted:', stream);
          this.ngZone.run(() => {
            if (this.remoteAudioRef?.nativeElement) {
              this.remoteAudioRef.nativeElement.srcObject = stream;
              this.remoteAudioRef.nativeElement.play().catch(err => console.error('[Doctor Component] Play remote audio from stream failed:', err));
            }
          });
        });

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
        await lastValueFrom(this.clinicalSessionService.endSession(this.sessionId));
        const audioBlob = await this.orchestrator.endCall();

        this.state = 'Uploading';
        this.cdr.detectChanges();
        await lastValueFrom(this.transcriptService.upload(this.sessionId, audioBlob));

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
      next: (res) => {
        this.ngZone.run(() => {
          if (res.status === 'Completed') {
            this.router.navigate(['/clinical-session', this.sessionId, 'report']);
          } else if (res.status === 'Failed') {
            this.state = 'Error';
            this.errorMessage = 'Transcription failed. You can retry from the session details page.';
            this.errorDetail = res.errorDetail ?? '';
            this.cdr.detectChanges();
          }
        });
      },
      error: () => {
        this.ngZone.run(() => {
          this.state = 'Error';
          this.errorMessage = 'Lost connection while checking transcription status.';
          this.errorDetail = '';
          this.cdr.detectChanges();
        });
      }
    });
  }

  loadNotes(): void {
    if (!this.sessionId) return;
    this.clinicalSessionService.getNotes(this.sessionId).subscribe({
      next: (list) => { this.ngZone.run(() => { this.notes = list; this.cdr.detectChanges(); }); },
      error: () => { }
    });
  }

  saveNote(): void {
    const content = this.sessionNotes.trim();
    if (!content || !this.sessionId || this.isSavingNote) return;
    this.isSavingNote = true;
    this.notesError = '';
    this.cdr.detectChanges();

    this.clinicalSessionService.addNote(this.sessionId, content).subscribe({
      next: (res) => {
        this.ngZone.run(() => {
          const newNote: SessionNote = {
            id: res.id, clinicalSessionId: this.sessionId, content,
            noteType: 'Quick', createdAt: new Date().toISOString(), updatedAt: null
          };
          this.notes = [...this.notes, newNote];
          this.sessionNotes = '';
          this.isSavingNote = false;
          this.cdr.detectChanges();
        });
      },
      error: () => {
        this.ngZone.run(() => {
          this.notesError = 'Failed to save note. Please try again.';
          this.isSavingNote = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  clearNotes(): void { this.sessionNotes = ''; }

  startEdit(note: SessionNote): void {
    this.editingNoteId = note.id;
    this.editingContent = note.content;
  }

  cancelEdit(): void {
    this.editingNoteId = null;
    this.editingContent = '';
  }

  saveEdit(note: SessionNote): void {
    const content = this.editingContent.trim();
    if (!content || this.isUpdatingNote) return;
    this.isUpdatingNote = true;
    this.cdr.detectChanges();

    this.clinicalSessionService.updateNote(note.clinicalSessionId, note.id, content).subscribe({
      next: () => {
        this.ngZone.run(() => {
          this.notes = this.notes.map(n =>
            n.id === note.id ? { ...n, content, updatedAt: new Date().toISOString() } : n
          );
          this.editingNoteId = null;
          this.editingContent = '';
          this.isUpdatingNote = false;
          this.cdr.detectChanges();
        });
      },
      error: () => {
        this.ngZone.run(() => { this.isUpdatingNote = false; this.cdr.detectChanges(); });
      }
    });
  }

  private startDurationTimer(): void {
    this.callDurationSeconds = 0;
    this.timerSub = interval(1000).subscribe(() => {
      this.ngZone.run(() => { this.callDurationSeconds++; this.cdr.detectChanges(); });
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
    if (err?.message === 'No supported audio recording MIME type found in this browser.') {
      return 'This browser cannot record audio in a supported format. Please try Chrome, Edge, or Firefox.';
    }
    return 'Could not start the session. Please try again.';
  }

  ngOnDestroy(): void {
    this.stopDurationTimer();
    this.pollSub?.unsubscribe();
    if (this.state === 'InCall') {
      this.orchestrator.endCall().catch(() => { });
    }
  }
}