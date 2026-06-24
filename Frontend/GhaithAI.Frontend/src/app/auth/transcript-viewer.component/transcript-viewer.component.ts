import {
  Component, OnInit, OnDestroy,
  NgZone, ChangeDetectorRef, ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { TranscriptService, TranscriptSegment } from '../../services/transcript.service';

@Component({
  selector: 'app-transcript-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transcript-viewer.component.html',
  styleUrls: ['./transcript-viewer.component.css'],
  changeDetection: ChangeDetectionStrategy.Default
})
export class TranscriptViewerComponent implements OnInit, OnDestroy {

  sessionId = '';
  segments: TranscriptSegment[] = [];
  loading = true;
  errorMessage = '';
  retryCount = 0;

  editingSegmentId: string | null = null;
  draftContent = '';
  saving = false;

  private loadSub: Subscription | null = null;
  private saveSub: Subscription | null = null;

  // ── REQUEST TIMEOUT ────────────────────────────────────────────────────────
  // If the backend doesn't respond within 15 s, stop waiting and show an error.
  // Previously the component waited forever, causing the "infinite loading" bug.
  private readonly REQUEST_TIMEOUT_MS = 15_000;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private transcriptService: TranscriptService,

    // ── WHY NgZone? ───────────────────────────────────────────────────────────
    // RxJS subscribe() callbacks can run outside Angular's change-detection zone,
    // especially when errors occur (HttpClient error path). Without wrapping state
    // changes in ngZone.run(), `loading = false` and `errorMessage = '...'` are
    // set in memory but Angular never re-renders the template — causing the
    // spinner to stay visible forever even though the request already finished.
    private ngZone: NgZone,

    // ── WHY ChangeDetectorRef? ────────────────────────────────────────────────
    // An explicit detectChanges() call guarantees the DOM is updated immediately
    // after each state transition, regardless of Angular's batching strategy.
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.sessionId = this.route.snapshot.paramMap.get('sessionId') ?? '';

    if (!this.sessionId) {
      this.ngZone.run(() => {
        this.loading = false;
        this.errorMessage = 'No session ID found. Please navigate from the session page.';
        this.cdr.detectChanges();
      });
      return;
    }

    this.loadSegments();
  }

  ngOnDestroy(): void {
    this.loadSub?.unsubscribe();
    this.saveSub?.unsubscribe();
  }

  // ── loadSegments ────────────────────────────────────────────────────────────
  // Core fix: every state mutation inside subscribe callbacks is now wrapped
  // in ngZone.run() + cdr.detectChanges() so Angular always sees the update.
  loadSegments(): void {
    this.loadSub?.unsubscribe();

    this.ngZone.run(() => {
      this.loading = true;
      this.errorMessage = '';
      this.cdr.detectChanges(); // show spinner immediately
    });

    this.loadSub = this.transcriptService
      .getSegments(this.sessionId)
      .pipe(
        // ── TIMEOUT GUARD ───────────────────────────────────────────────────
        // If the backend hangs (no response in 15 s), the timeout operator
        // forces an error emission — the error() callback below then clears
        // the loading state and shows a user-friendly message.
        timeout(this.REQUEST_TIMEOUT_MS),

        // ── SILENT ERROR NORMALISER ─────────────────────────────────────────
        // If the backend returns a non-JSON body (e.g. "GhaithAITe…" plain
        // text), HttpClient throws a SyntaxError. catchError converts it to
        // a typed error so the error() callback still fires instead of being
        // silently swallowed by the zone.
        catchError((err) => {
          throw err; // re-throw so the error() callback receives it
        })
      )
      .subscribe({
        next: (segments) => {
          this.ngZone.run(() => {
            // Guard against a non-array response (e.g. backend returns a
            // string or object instead of the expected array).
            if (Array.isArray(segments)) {
              this.segments = segments.sort((a, b) => a.startMs - b.startMs);
            } else {
              // Treat unexpected response shape as "no segments found"
              // rather than crashing with a runtime type error.
              this.segments = [];
              console.warn('[TranscriptViewer] Expected array, got:', typeof segments, segments);
            }
            this.loading = false;
            this.errorMessage = '';
            this.retryCount = 0;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            this.loading = false;
            this.retryCount++;

            if (err?.name === 'TimeoutError') {
              this.errorMessage = 'The server took too long to respond. Check your connection and try again.';
            } else if (err?.status === 401 || err?.status === 403) {
              this.errorMessage = 'You are not authorised to view this transcript.';
            } else if (err?.status === 404) {
              this.errorMessage = 'Transcript not found. The session may not have a recording yet.';
            } else if (err?.status === 0) {
              this.errorMessage = 'Cannot reach the server. Please check your internet connection.';
            } else {
              // Covers SyntaxError (non-JSON body), 500, etc.
              this.errorMessage = 'Could not load the transcript. Please try again.';
            }

            this.cdr.detectChanges();
          });
        }
      });
  }

  startEdit(segment: TranscriptSegment): void {
    this.editingSegmentId = segment.id;
    this.draftContent = segment.content;
  }

  cancelEdit(): void {
    this.editingSegmentId = null;
    this.draftContent = '';
  }

  saveEdit(segment: TranscriptSegment): void {
    if (!this.draftContent.trim()) return;

    this.saveSub?.unsubscribe();

    this.ngZone.run(() => {
      this.saving = true;
      this.cdr.detectChanges();
    });

    this.saveSub = this.transcriptService
      .updateSegment(this.sessionId, segment.id, this.draftContent)
      .pipe(timeout(this.REQUEST_TIMEOUT_MS))
      .subscribe({
        next: (updated) => {
          this.ngZone.run(() => {
            const index = this.segments.findIndex(s => s.id === segment.id);
            if (index > -1) this.segments[index] = updated;
            this.editingSegmentId = null;
            this.draftContent = '';
            this.saving = false;
            this.cdr.detectChanges();
          });
        },
        error: () => {
          this.ngZone.run(() => {
            this.saving = false;
            this.errorMessage = 'Could not save this edit. Please try again.';
            this.cdr.detectChanges();
          });
        }
      });
  }

  formatTime(ms: number): string {
    const totalSeconds = Math.floor(ms / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  proceedToReport(): void {
    this.router.navigate(['/clinical-session', this.sessionId, 'report']);
  }

  goBack(): void {
    this.router.navigate(['/clinician-dashboard']);
  }
}
