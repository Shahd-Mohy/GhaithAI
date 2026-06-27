import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface StartSessionResponse {
  id: string;
  message: string;
}

// ── Matches the backend's SessionResponseDto (ClinicalSessionService.cs) ──
// NOTE: doctorName is NOT currently returned by GetSessionByIdAsync — the
// backend only maps DoctorId (a DoctorProfile.Id guid), not a display name.
// Until the backend joins DoctorProfile → ApplicationUser to include a name,
// treat doctorName as optional and fall back to a generic label in the UI.
export interface SessionResponse {
  id: string;
  bookingId: string;
  doctorId: string;
  patientId: string;
  startedAt: string;
  endedAt: string | null;
  durationMinutes: number | null;
  status: string;
  sessionType: string;
  chiefComplaint: string | null;
  provider: string | null;
  videoRoomId: string | null;
  videoRoomUrl: string | null;
  doctorName?: string;
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

  constructor(private http: HttpClient) { }

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

  // GET /api/sessions/{sessionId}
  // Used by the patient session page to look up the doctor and confirm the
  // session exists before joining the call.
  getSession(sessionId: string): Observable<SessionResponse> {
    return this.http.get<SessionResponse>(`${this.baseUrl}/${sessionId}`);
  }

  // GET /api/sessions?bookingId={bookingId}
  // Looks up the clinical session created by the doctor for a given booking.
  // The patient uses this to discover their sessionId from their bookingId.
  getSessionByBookingId(bookingId: string): Observable<SessionResponse | null> {
    return this.http.get<SessionResponse | null>(
      `${this.baseUrl}/by-booking/${bookingId}`
    );
  }

  // GET /api/sessions/my
  // Returns all clinical sessions where the current user is the patient.
  // Used by the patient dashboard to find any active/recent session.
  getMyActiveSessions(): Observable<SessionResponse[]> {
    return this.http.get<SessionResponse[]>(`${this.baseUrl}/my`);
  }

  // GET /api/sessions/doctor
  // Returns clinical sessions owned by the current authenticated clinician.
  getDoctorSessions(status?: string, date?: string): Observable<SessionResponse[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    if (date) params = params.set('date', date);
    return this.http.get<SessionResponse[]>(`${this.baseUrl}/doctor`, { params });
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
