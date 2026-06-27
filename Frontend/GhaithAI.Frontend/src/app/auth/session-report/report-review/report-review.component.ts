import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  Output,
  SimpleChanges
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import Swal from 'sweetalert2';
import {
  ReportService,
  SessionReportResponse,
  SessionReportVersion,
  UpdateReportDto
} from '../../../services/report.service';
import {
  ClinicalSessionService,
  SessionNote,
  SessionResponse
} from '../../../services/clinical-session.service';
import {
  TranscriptSegment,
  TranscriptService
} from '../../../services/transcript.service';

type EvidenceTab = 'notes' | 'transcript' | 'versions';

@Component({
  selector: 'app-report-review',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './report-review.component.html',
  styleUrls: ['./report-review.component.css']
})
export class ReportReviewComponent implements OnInit, OnChanges, OnDestroy {
  @Input({ required: true }) sessionId = '';
  @Input() reportId: string | null = null;
  @Input() embedded = false;
  @Output() reportChanged = new EventEmitter<SessionReportResponse | null>();

  session: SessionResponse | null = null;
  report: SessionReportResponse | null = null;

  loading = true;
  loadError = '';
  noReport = false;
  generating = false;
  generateError = '';

  editing = false;
  saving = false;
  saveError = '';
  changeNote = '';
  soap = {
    subjective: '',
    objective: '',
    assessment: '',
    plan: ''
  };

  approving = false;
  locking = false;
  exporting = false;
  actionError = '';
  actionSuccess = '';

  activeTab: EvidenceTab = 'notes';
  notes: SessionNote[] = [];
  notesLoading = false;
  notesError = '';

  transcript: TranscriptSegment[] = [];
  transcriptLoading = false;
  transcriptError = '';

  versions: SessionReportVersion[] = [];
  versionsLoading = false;
  versionsError = '';
  selectedVersion: SessionReportVersion | null = null;

  private readonly destroy$ = new Subject<void>();
  private initialized = false;

  constructor(
    private reports: ReportService,
    private sessions: ClinicalSessionService,
    private transcripts: TranscriptService
  ) {}

  ngOnInit(): void {
    this.initialized = true;
    this.load();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.initialized) return;
    if (changes['sessionId'] || changes['reportId']) {
      this.load();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  load(): void {
    if (!this.sessionId) {
      this.loading = false;
      this.loadError = 'Missing session id.';
      return;
    }

    this.loading = true;
    this.loadError = '';
    this.noReport = false;
    this.generateError = '';
    this.report = null;
    this.session = null;
    this.editing = false;
    this.actionError = '';
    this.actionSuccess = '';

    this.sessions.getSession(this.sessionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (session) => {
          this.session = session;
          this.loadReport();
          this.loadNotes();
          this.loadTranscript();
        },
        error: (err) => {
          this.loading = false;
          this.loadError = err?.error?.message || 'Could not load session context.';
        }
      });
  }

  generateReport(): void {
    if (!this.session || this.generating) return;

    this.generating = true;
    this.generateError = '';
    this.actionError = '';
    this.actionSuccess = '';

    this.reports.generate(this.session.id, this.session.doctorId, this.session.patientId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (report) => {
          this.report = report;
          this.noReport = false;
          this.generating = false;
          this.resetSoap();
          this.loadVersions();
          this.reportChanged.emit(report);
        },
        error: (err) => {
          this.generating = false;
          this.generateError = this.extractError(err, 'Could not generate report.');
          this.reportChanged.emit(null);
        }
      });
  }

  startEdit(): void {
    if (!this.report || this.isLocked) return;
    this.resetSoap();
    this.changeNote = '';
    this.saveError = '';
    this.editing = true;
  }

  cancelEdit(): void {
    this.editing = false;
    this.saveError = '';
    this.changeNote = '';
    this.resetSoap();
  }

  saveEdit(): void {
    if (!this.report || !this.session || this.saving || this.isLocked) return;

    this.saving = true;
    this.saveError = '';

    const dto: UpdateReportDto = {
      soapSubjective: this.soap.subjective,
      soapObjective: this.soap.objective,
      soapAssessment: this.soap.assessment,
      soapPlan: this.soap.plan,
      changeNote: this.changeNote.trim() || `SOAP update for version ${this.report.versionNumber + 1}`
    };

    this.reports.update(this.report.reportId, this.session.doctorId, dto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updated) => {
          this.report = updated;
          this.saving = false;
          this.editing = false;
          this.changeNote = '';
          this.actionSuccess = 'SOAP changes saved.';
          this.resetSoap();
          this.loadVersions();
          this.reportChanged.emit(updated);
        },
        error: (err) => {
          this.saving = false;
          this.saveError = this.extractError(err, 'Could not save changes.');
        }
      });
  }

  approveReport(): void {
    if (!this.report || !this.session || this.approving) return;

    Swal.fire({
      title: 'Approve report?',
      text: 'This marks the draft as clinically reviewed.',
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Approve',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#059669'
    }).then((result) => {
      if (!result.isConfirmed || !this.report || !this.session) return;

      this.approving = true;
      this.actionError = '';
      this.actionSuccess = '';

      this.reports.approve(this.report.reportId, this.session.doctorId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updated) => {
            this.report = updated;
            this.approving = false;
            this.actionSuccess = 'Report approved.';
            this.reportChanged.emit(updated);
          },
          error: (err) => {
            this.approving = false;
            this.actionError = this.extractError(err, 'Approval failed.');
          }
        });
    });
  }

  lockReport(): void {
    if (!this.report || !this.session || this.locking) return;

    Swal.fire({
      title: 'Lock report permanently?',
      text: 'Locked reports become read-only and cannot be edited.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Lock report',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#0D1B3E'
    }).then((result) => {
      if (!result.isConfirmed || !this.report || !this.session) return;

      this.locking = true;
      this.actionError = '';
      this.actionSuccess = '';

      this.reports.lock(this.report.reportId, this.session.doctorId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updated) => {
            this.report = updated;
            this.locking = false;
            this.editing = false;
            this.actionSuccess = 'Report locked.';
            this.reportChanged.emit(updated);
          },
          error: (err) => {
            this.locking = false;
            this.actionError = this.extractError(err, 'Lock failed.');
          }
        });
    });
  }

  exportPdf(): void {
    if (!this.report || this.exporting) return;

    this.exporting = true;
    this.actionError = '';

    this.reports.exportPdf(this.report.reportId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = `GhaithAI_Report_${this.report!.reportId.slice(0, 8)}.pdf`;
          anchor.click();
          URL.revokeObjectURL(url);
          this.exporting = false;
        },
        error: () => {
          this.exporting = false;
          this.actionError = 'PDF export failed.';
        }
      });
  }

  setTab(tab: EvidenceTab): void {
    this.activeTab = tab;
  }

  selectVersion(version: SessionReportVersion): void {
    this.selectedVersion = version;
  }

  get isDraft(): boolean {
    return this.report?.status === 'Draft';
  }

  get isApproved(): boolean {
    return this.report?.status === 'Approved';
  }

  get isLocked(): boolean {
    return this.report?.status === 'Locked';
  }

  get riskClass(): string {
    const tier = (this.report?.riskTier || 'low').toLowerCase();
    if (tier.includes('critical')) return 'risk-critical';
    if (tier.includes('high')) return 'risk-high';
    if (tier.includes('medium')) return 'risk-medium';
    return 'risk-low';
  }

  get patientLabel(): string {
    return this.report?.patientId || this.session?.patientId || 'Patient';
  }

  initials(value: string): string {
    return value
      .split(/[\s-]+/)
      .filter(Boolean)
      .map(part => part[0])
      .join('')
      .slice(0, 2)
      .toUpperCase() || 'PT';
  }

  formatDuration(minutes?: number | null): string {
    if (minutes == null) return 'Not ended';
    if (minutes < 60) return `${minutes} min`;
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m ? `${h}h ${m}m` : `${h}h`;
  }

  transcriptTime(ms: number): string {
    const totalSeconds = Math.floor(ms / 1000);
    const minutes = Math.floor(totalSeconds / 60).toString().padStart(2, '0');
    const seconds = (totalSeconds % 60).toString().padStart(2, '0');
    return `${minutes}:${seconds}`;
  }

  prettySnapshot(version: SessionReportVersion | null): string {
    if (!version) return '';
    try {
      return JSON.stringify(JSON.parse(version.snapshotJson), null, 2);
    } catch {
      return version.snapshotJson;
    }
  }

  private loadReport(): void {
    const request = this.reportId
      ? this.reports.getById(this.reportId)
      : this.reports.getBySession(this.sessionId);

    request.pipe(takeUntil(this.destroy$)).subscribe({
      next: (report) => {
        this.report = report;
        this.noReport = false;
        this.loading = false;
        this.resetSoap();
        this.loadVersions();
        this.reportChanged.emit(report);
      },
      error: (err) => {
        this.loading = false;
        if (err?.status === 404) {
          this.noReport = true;
          this.reportChanged.emit(null);
          return;
        }
        this.loadError = this.extractError(err, 'Could not load report.');
      }
    });
  }

  private loadNotes(): void {
    this.notesLoading = true;
    this.notesError = '';

    this.sessions.getNotes(this.sessionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (notes) => {
          this.notes = Array.isArray(notes) ? notes : [];
          this.notesLoading = false;
        },
        error: () => {
          this.notes = [];
          this.notesLoading = false;
          this.notesError = 'Could not load notes.';
        }
      });
  }

  private loadTranscript(): void {
    this.transcriptLoading = true;
    this.transcriptError = '';

    this.transcripts.getSegments(this.sessionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (segments) => {
          this.transcript = (Array.isArray(segments) ? segments : [])
            .sort((a, b) => a.startMs - b.startMs);
          this.transcriptLoading = false;
        },
        error: () => {
          this.transcript = [];
          this.transcriptLoading = false;
          this.transcriptError = 'Transcript is not available yet.';
        }
      });
  }

  private loadVersions(): void {
    if (!this.report) return;

    this.versionsLoading = true;
    this.versionsError = '';
    this.selectedVersion = null;

    this.reports.getVersions(this.report.reportId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (versions) => {
          this.versions = (Array.isArray(versions) ? versions : [])
            .sort((a, b) => b.versionNumber - a.versionNumber);
          this.selectedVersion = this.versions[0] || null;
          this.versionsLoading = false;
        },
        error: () => {
          this.versions = [];
          this.versionsLoading = false;
          this.versionsError = 'Could not load version history.';
        }
      });
  }

  private resetSoap(): void {
    this.soap = {
      subjective: this.report?.soapSubjective || '',
      objective: this.report?.soapObjective || '',
      assessment: this.report?.soapAssessment || '',
      plan: this.report?.soapPlan || ''
    };
  }

  private extractError(err: any, fallback: string): string {
    return err?.error?.error || err?.error?.message || err?.message || fallback;
  }
}
