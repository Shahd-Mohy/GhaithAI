// src/app/services/report.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SessionReportResponse {
  reportId: string;
  sessionId: string;
  status: 'Draft' | 'Approved' | 'Locked';
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
    return this.http.get<SessionReportResponse>(
      `${this.base}/by-session/${sessionId}`
    );
  }

  getById(reportId: string): Observable<SessionReportResponse> {
    return this.http.get<SessionReportResponse>(`${this.base}/${reportId}`);
  }

  update(reportId: string, doctorId: string, dto: UpdateReportDto): Observable<SessionReportResponse> {
    return this.http.put<SessionReportResponse>(
      `${this.base}/${reportId}?doctorId=${doctorId}`, dto
    );
  }

  approve(reportId: string, doctorId: string): Observable<SessionReportResponse> {
    return this.http.post<SessionReportResponse>(
      `${this.base}/${reportId}/approve?doctorId=${doctorId}`, {}
    );
  }

  lock(reportId: string, doctorId: string): Observable<SessionReportResponse> {
    return this.http.post<SessionReportResponse>(
      `${this.base}/${reportId}/lock?doctorId=${doctorId}`, {}
    );
  }

  generate(sessionId: string, doctorId: string): Observable<SessionReportResponse> {
  return this.http.post<SessionReportResponse>(
    `${this.base}/${sessionId}/generate?doctorId=${doctorId}&patientId=auto`,
    {}
  );
}

  exportPdf(reportId: string): Observable<Blob> {
    return this.http.get(
      `${this.base}/${reportId}/export-pdf`,
      { responseType: 'blob' }
    );
  }
}