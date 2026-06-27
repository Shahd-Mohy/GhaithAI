import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export type ReportStatus = 'Draft' | 'Approved' | 'Locked';

export interface SessionReportResponse {
  reportId: string;
  sessionId: string;
  status: ReportStatus;
  clinicianId: string;
  patientId: string;
  riskTier: string;
  siPresent: boolean;
  suicidalIdeationDetails?: string;
  riskNarrative?: string;
  soapSubjective?: string;
  soapObjective?: string;
  soapAssessment?: string;
  soapPlan?: string;
  chiefComplaintPrimary?: string;
  chiefComplaintDuration?: string;
  differentialConsiderations?: string[];
  approvedAt?: string;
  approvedBy?: string;
  versionNumber: number;
  generatedAt: string;
}

export interface SessionReportVersion {
  versionId: string;
  versionNumber: number;
  changeSummary?: string;
  snapshotJson: string;
  createdAt: string;
}

export interface UpdateReportDto {
  soapSubjective?: string;
  soapObjective?: string;
  soapAssessment?: string;
  soapPlan?: string;
  changeNote?: string;
}

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly base = `${environment.apiUrl}/SessionReports`;

  constructor(private http: HttpClient) {}

  getBySession(sessionId: string): Observable<SessionReportResponse> {
    return this.http.get<SessionReportResponse>(`${this.base}/by-session/${sessionId}`);
  }

  getById(reportId: string): Observable<SessionReportResponse> {
    return this.http.get<SessionReportResponse>(`${this.base}/${reportId}`);
  }

  generate(sessionId: string, doctorId: string, patientId: string): Observable<SessionReportResponse> {
    const params = new HttpParams()
      .set('doctorId', doctorId)
      .set('patientId', patientId);

    return this.http.post<SessionReportResponse>(
      `${this.base}/${sessionId}/generate`,
      {},
      { params }
    );
  }

  update(reportId: string, doctorId: string, dto: UpdateReportDto): Observable<SessionReportResponse> {
    const params = new HttpParams().set('doctorId', doctorId);
    return this.http.put<SessionReportResponse>(`${this.base}/${reportId}`, dto, { params });
  }

  approve(reportId: string, doctorId: string): Observable<SessionReportResponse> {
    const params = new HttpParams().set('doctorId', doctorId);
    return this.http.post<SessionReportResponse>(`${this.base}/${reportId}/approve`, {}, { params });
  }

  lock(reportId: string, doctorId: string): Observable<SessionReportResponse> {
    const params = new HttpParams().set('doctorId', doctorId);
    return this.http.post<SessionReportResponse>(`${this.base}/${reportId}/lock`, {}, { params });
  }

  getVersions(reportId: string): Observable<SessionReportVersion[]> {
    return this.http.get<SessionReportVersion[]>(`${this.base}/${reportId}/versions`);
  }

  getVersion(reportId: string, versionNumber?: number): Observable<SessionReportVersion> {
    const suffix = versionNumber ? `/${versionNumber}` : '';
    return this.http.get<SessionReportVersion>(`${this.base}/${reportId}/versions${suffix}`);
  }

  exportPdf(reportId: string): Observable<Blob> {
    return this.http.get(`${this.base}/${reportId}/export-pdf`, { responseType: 'blob' });
  }
}
