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

// ── Session Notes ─────────────────────────────────────────────
export interface SessionNote {
  id: string;
  clinicalSessionId: string;
  content: string;
  noteType: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface AddNoteResponse {
  id: string;
  message: string;
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

  // GET /api/sessions/{sessionId}/notes
  getNotes(sessionId: string): Observable<SessionNote[]> {
    return this.http.get<SessionNote[]>(`${this.baseUrl}/${sessionId}/notes`);
  }

  // POST /api/sessions/{sessionId}/notes
  addNote(sessionId: string, content: string, noteType: string = 'Quick'): Observable<AddNoteResponse> {
    return this.http.post<AddNoteResponse>(
      `${this.baseUrl}/${sessionId}/notes`,
      { content, noteType }
    );
  }

  // PUT /api/sessions/{sessionId}/notes/{noteId}
  updateNote(sessionId: string, noteId: string, content: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${this.baseUrl}/${sessionId}/notes/${noteId}`,
      { content }
    );
  }
}