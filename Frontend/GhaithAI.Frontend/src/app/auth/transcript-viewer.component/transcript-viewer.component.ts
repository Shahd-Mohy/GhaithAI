import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranscriptService, TranscriptSegment } from '../../services/transcript.service';

@Component({
  selector: 'app-transcript-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transcript-viewer.component.html',
  styleUrls: ['./transcript-viewer.component.css']
})
export class TranscriptViewerComponent implements OnInit {

  sessionId = '';
  segments: TranscriptSegment[] = [];
  loading = true;
  errorMessage = '';

  editingSegmentId: string | null = null;
  draftContent = '';
  saving = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private transcriptService: TranscriptService
  ) {}

  ngOnInit(): void {
    this.sessionId = this.route.snapshot.paramMap.get('sessionId') ?? '';
    this.loadSegments();
  }

  loadSegments(): void {
    this.loading = true;
    this.transcriptService.getSegments(this.sessionId).subscribe({
      next: (segments) => {
        this.segments = segments.sort((a, b) => a.startMs - b.startMs);
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Could not load the transcript. Please try again.';
        this.loading = false;
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

    this.saving = true;

    this.transcriptService.updateSegment(this.sessionId, segment.id, this.draftContent).subscribe({
      next: (updated) => {
        const index = this.segments.findIndex(s => s.id === segment.id);
        if (index > -1) this.segments[index] = updated;

        this.editingSegmentId = null;
        this.draftContent = '';
        this.saving = false;
      },
      error: () => {
        this.saving = false;
        this.errorMessage = 'Could not save this edit. Please try again.';
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
}
