import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ReportService, SessionReportResponse, UpdateReportDto } from '../../services/report.service';

@Component({
  selector: 'app-session-report',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './session-report.html',
  styleUrls: ['./session-report.css']
})
export class SessionReportComponent implements OnInit {

  sessionId = '';
  doctorId  = '';

  report: SessionReportResponse | null = null;
  loading      = true;
  generating   = false;  // ← حالة الـ generate
  error        = '';

  editing   = false;
  saving    = false;
  saveError = '';
  soap = { subjective: '', objective: '', assessment: '', plan: '' };

  approving = false;
  locking   = false;
  exporting = false;
  actionError   = '';
  actionSuccess = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private reportService: ReportService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // جيب sessionId من الـ URL
    this.sessionId = this.route.snapshot.paramMap.get('sessionId') ?? '';

    // جيب doctorId من الـ JWT / localStorage
    const user = JSON.parse(localStorage.getItem('user') ?? '{}');
    this.doctorId = user.doctorId ?? user.id ?? '';

    this.loadOrGenerate();
  }

  // ── Load أو Generate ─────────────────────────────────────────────────────
  loadOrGenerate(): void {
    this.loading = true;
    this.error   = '';

    // حاول تجيب الريبورت الموجود الأول
    this.reportService.getBySession(this.sessionId).subscribe({
      next: (r) => {
        this.report  = r;
        this.loading = false;
        this.resetSoap();
        this.cdr.detectChanges();
      },
      error: (err) => {
        if (err.status === 404) {
          // مفيش ريبورت → اعمل generate
          this.generateReport();
        } else {
          this.error   = err.error?.error || 'Failed to load report.';
          this.loading = false;
          this.cdr.detectChanges();
        }
      }
    });
  }

  generateReport(): void {
    this.generating = true;
    this.loading    = false; // وقف الـ loading spinner العادي
    this.cdr.detectChanges();

    this.reportService.generate(this.sessionId, this.doctorId).subscribe({
      next: (r) => {
        this.report     = r;
        this.generating = false;
        this.resetSoap();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error      = err.error?.error || 'Failed to generate report.';
        this.generating = false;
        this.cdr.detectChanges();
      }
    });
  }

  // ── SOAP Edit ─────────────────────────────────────────────────────────────
  startEdit(): void { this.resetSoap(); this.editing = true; this.saveError = ''; }
  cancelEdit(): void { this.editing = false; this.resetSoap(); }

  saveEdit(): void {
    if (!this.report) return;
    this.saving = true; this.saveError = '';

    const dto: UpdateReportDto = {
      soapSubjective: this.soap.subjective,
      soapObjective:  this.soap.objective,
      soapAssessment: this.soap.assessment,
      soapPlan:       this.soap.plan,
      changeNote:     `Doctor edit — v${this.report.versionNumber + 1}`
    };

    this.reportService.update(this.report.reportId, this.doctorId, dto).subscribe({
      next: (u) => { this.report = u; this.editing = false; this.saving = false; this.resetSoap(); this.cdr.detectChanges(); },
      error: (e) => { this.saveError = e.error?.error || 'Save failed.'; this.saving = false; this.cdr.detectChanges(); }
    });
  }

  // ── Approve ───────────────────────────────────────────────────────────────
  approve(): void {
    if (!this.report) return;
    this.approving = true; this.actionError = ''; this.actionSuccess = '';
    this.reportService.approve(this.report.reportId, this.doctorId).subscribe({
      next: (u) => { this.report = u; this.approving = false; this.actionSuccess = 'Report approved successfully.'; this.cdr.detectChanges(); },
      error: (e) => { this.actionError = e.error?.error || 'Approval failed.'; this.approving = false; this.cdr.detectChanges(); }
    });
  }

  // ── Lock ──────────────────────────────────────────────────────────────────
  lock(): void {
    if (!this.report) return;
    this.locking = true; this.actionError = ''; this.actionSuccess = '';
    this.reportService.lock(this.report.reportId, this.doctorId).subscribe({
      next: (u) => { this.report = u; this.locking = false; this.actionSuccess = 'Report locked permanently.'; this.cdr.detectChanges(); },
      error: (e) => { this.actionError = e.error?.error || 'Lock failed.'; this.locking = false; this.cdr.detectChanges(); }
    });
  }

  // ── Export PDF ────────────────────────────────────────────────────────────
  exportPdf(): void {
    if (!this.report) return;
    this.exporting = true;
    this.reportService.exportPdf(this.report.reportId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a   = document.createElement('a');
        a.href = url; a.download = `GhaithAI_Report_${this.report!.reportId.slice(0,8)}.pdf`;
        a.click(); URL.revokeObjectURL(url);
        this.exporting = false; this.cdr.detectChanges();
      },
      error: () => { this.actionError = 'PDF export failed.'; this.exporting = false; this.cdr.detectChanges(); }
    });
  }

  get isLocked():  boolean { return this.report?.status === 'Locked'; }
  get isDraft():   boolean { return this.report?.status === 'Draft'; }
  get isApproved():boolean { return this.report?.status === 'Approved'; }

  get riskColor(): string {
    switch (this.report?.riskTier?.toUpperCase()) {
      case 'CRITICAL': return 'risk-critical';
      case 'HIGH':     return 'risk-high';
      case 'MEDIUM':   return 'risk-medium';
      default:         return 'risk-low';
    }
  }

  goBack(): void { this.router.navigate(['/clinical-session', this.sessionId, 'transcript']); }

  private resetSoap(): void {
    if (!this.report) return;
    this.soap = {
      subjective: this.report.soapSubjective ?? '',
      objective:  this.report.soapObjective  ?? '',
      assessment: this.report.soapAssessment ?? '',
      plan:       this.report.soapPlan       ?? ''
    };
  }
}