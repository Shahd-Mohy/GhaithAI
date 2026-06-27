import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, of, Subject } from 'rxjs';
import { catchError, takeUntil } from 'rxjs/operators';
import {
  ClinicalSessionService,
  SessionResponse
} from '../../../../../services/clinical-session.service';
import {
  ReportStatus,
  ReportService,
  SessionReportResponse
} from '../../../../../services/report.service';
import { ReportReviewComponent } from '../../../../session-report/report-review/report-review.component';

type ReportFilter = 'all' | 'needs-report' | 'draft' | 'approved' | 'locked';
type RiskFilter = 'all' | 'low' | 'medium' | 'high' | 'critical';

interface ReportListItem {
  session: SessionResponse;
  report: SessionReportResponse | null;
}

@Component({
  selector: 'app-clinical-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, ReportReviewComponent],
  templateUrl: './clinical-reports.component.html',
  styleUrls: ['./clinical-reports.component.css']
})
export class ClinicalReportsComponent implements OnInit, OnDestroy {
  items: ReportListItem[] = [];
  filtered: ReportListItem[] = [];
  selected: ReportListItem | null = null;

  loading = true;
  errorMessage = '';
  searchQuery = '';
  statusFilter: ReportFilter = 'all';
  riskFilter: RiskFilter = 'all';

  private readonly destroy$ = new Subject<void>();

  constructor(
    private sessions: ClinicalSessionService,
    private reports: ReportService
  ) {}

  ngOnInit(): void {
    this.loadReports();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadReports(): void {
    this.loading = true;
    this.errorMessage = '';
    this.items = [];
    this.filtered = [];
    this.selected = null;

    this.sessions.getDoctorSessions()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (sessions) => this.loadReportStates(sessions || []),
        error: () => {
          this.loading = false;
          this.errorMessage = 'Could not load your clinical sessions.';
        }
      });
  }

  setStatusFilter(filter: ReportFilter): void {
    this.statusFilter = filter;
    this.applyFilters();
  }

  setRiskFilter(filter: RiskFilter): void {
    this.riskFilter = filter;
    this.applyFilters();
  }

  onSearch(): void {
    this.applyFilters();
  }

  selectItem(item: ReportListItem): void {
    this.selected = item;
  }

  onReportChanged(report: SessionReportResponse | null): void {
    if (!this.selected) return;

    this.items = this.items.map(item =>
      item.session.id === this.selected!.session.id
        ? { ...item, report }
        : item
    );

    const updated = this.items.find(item => item.session.id === this.selected!.session.id) || null;
    this.selected = updated;
    this.applyFilters(false);
  }

  reportStatus(item: ReportListItem): ReportStatus | 'Needs report' {
    return item.report?.status || 'Needs report';
  }

  riskLabel(item: ReportListItem): string {
    return item.report?.riskTier || 'Not assessed';
  }

  patientLabel(item: ReportListItem): string {
    return item.report?.patientId || item.session.patientId || 'Patient';
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

  statusClass(item: ReportListItem): string {
    return item.report?.status?.toLowerCase() || 'needs';
  }

  riskClass(item: ReportListItem): string {
    const risk = (item.report?.riskTier || '').toLowerCase();
    if (risk.includes('critical')) return 'critical';
    if (risk.includes('high')) return 'high';
    if (risk.includes('medium')) return 'medium';
    if (risk.includes('low')) return 'low';
    return 'none';
  }

  countBy(filter: ReportFilter): number {
    if (filter === 'all') return this.items.length;
    if (filter === 'needs-report') return this.items.filter(item => !item.report).length;
    return this.items.filter(item => item.report?.status.toLowerCase() === filter).length;
  }

  private loadReportStates(sessions: SessionResponse[]): void {
    const sortedSessions = [...sessions].sort((a, b) =>
      new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime()
    );

    if (!sortedSessions.length) {
      this.loading = false;
      return;
    }

    const lookups = sortedSessions.map(session =>
      this.reports.getBySession(session.id).pipe(
        catchError(() => of(null))
      )
    );

    forkJoin(lookups)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (reports) => {
          this.items = sortedSessions.map((session, index) => ({
            session,
            report: reports[index]
          }));
          this.applyFilters();
          this.selected = this.filtered[0] || null;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.errorMessage = 'Could not load report states.';
        }
      });
  }

  private applyFilters(syncSelection = true): void {
    const query = this.searchQuery.trim().toLowerCase();

    this.filtered = this.items.filter(item => {
      const statusMatch =
        this.statusFilter === 'all' ||
        (this.statusFilter === 'needs-report' && !item.report) ||
        item.report?.status.toLowerCase() === this.statusFilter;

      const risk = (item.report?.riskTier || '').toLowerCase();
      const riskMatch =
        this.riskFilter === 'all' ||
        (this.riskFilter === 'high' && (risk.includes('high') || risk.includes('critical'))) ||
        risk.includes(this.riskFilter);

      const text = [
        item.session.patientId,
        item.session.chiefComplaint,
        item.session.sessionType,
        item.session.status,
        item.report?.patientId,
        item.report?.chiefComplaintPrimary,
        item.report?.riskTier,
        item.report?.status
      ].filter(Boolean).join(' ').toLowerCase();

      const queryMatch = !query || text.includes(query);
      return statusMatch && riskMatch && queryMatch;
    });

    if (syncSelection && this.selected && !this.filtered.some(item => item.session.id === this.selected!.session.id)) {
      this.selected = this.filtered[0] || null;
    }
  }
}
