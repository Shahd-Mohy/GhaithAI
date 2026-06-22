import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface StartSessionResponse {
  sessionId: string;
  doctorId: string;
  patientId: string;
  status: string;
}

export interface EndSessionResponse {
  sessionId: string;
  status: string;
  durationMinutes: number;
}

@Injectable({ providedIn: 'root' })
export class ClinicalSessionService {

  private readonly baseUrl = `${environment.apiUrl}/sessions`;

  constructor(private http: HttpClient) {}

  startSession(bookingId: string): Observable<StartSessionResponse> {
    return this.http.post<StartSessionResponse>(`${this.baseUrl}/start`, { bookingId });
  }

  endSession(sessionId: string): Observable<EndSessionResponse> {
    return this.http.put<EndSessionResponse>(`${this.baseUrl}/${sessionId}/end`, {});
  }
}
