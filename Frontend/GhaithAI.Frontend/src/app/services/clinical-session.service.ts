import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface StartSessionResponse {
  id: string;
  message: string;
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
    return this.http.post<StartSessionResponse>(
      `${this.baseUrl}/start`,
      {
        bookingId,
        sessionType: 'Voice',
        provider: '',
        chiefComplaint: '',
        videoRoomId: '',
        videoRoomUrl: ''
      },
      { observe: 'response' }
    ).pipe(
      map(res => res.body as StartSessionResponse)
    );
  }

  endSession(sessionId: string): Observable<EndSessionResponse> {
    return this.http.put<EndSessionResponse>(
      `${this.baseUrl}/${sessionId}/end`,
      {},
      { observe: 'response' }
    ).pipe(
      map(res => res.body as EndSessionResponse)
    );
  }
}