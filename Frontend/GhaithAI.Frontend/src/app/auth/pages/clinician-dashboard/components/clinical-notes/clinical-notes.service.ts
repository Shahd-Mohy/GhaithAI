import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';

export interface ClinicalNote {
  id: string;
  clinicalSessionId: string;
  content: string;
  noteType: string;        // e.g. "Quick"
  createdAt: string;
  updatedAt: string | null;
  patientId: string;
  patientDisplayName: string;
  sessionDisplayName: string;
  sessionType: string;     // e.g. "Voice"
}

@Injectable({ providedIn: 'root' })
export class ClinicalNotesService {

  private readonly base = `${environment.apiUrl}/sessions`;

  constructor(private http: HttpClient) {}

  /** All notes belonging to the current clinician */
  getMine(): Observable<ClinicalNote[]> {
    return this.http.get<ClinicalNote[]>(`${this.base}/notes/mine`);
  }

  /** PUT /api/sessions/{sessionId}/notes/{noteId} */
  updateNote(sessionId: string, noteId: string, content: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${this.base}/${sessionId}/notes/${noteId}`,
      { content }
    );
  }
}
