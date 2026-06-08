// File: src/services/chat.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  StartSessionRequest,
  SessionModel,
  PaginatedSessionsResponse,
  ChatHistoryResponse,
  UserChatRequest,
  SendMessageResponse,
} from '../types/chat.types';
import { environment } from '../environments/environment';

const API = `${environment.apiUrl}/Chat`;

@Injectable({ providedIn: 'root' })
export class ChatHttpService {
  private readonly http = inject(HttpClient);

  // ── Session Endpoints ────────────────────────────────────────────────────────

  /**
   * POST /api/Chat/sessions
   * Creates a new chat session. Auto-closes any existing active session on backend.
   */
  startSession(dto: StartSessionRequest): Observable<SessionModel> {
    return this.http.post<SessionModel>(`${API}/sessions`, dto);
  }

  /**
   * GET /api/Chat/sessions?page=1&pageSize=20
   * Returns paginated list of the user's sessions.
   */
  getSessions(page = 1, pageSize = 20): Observable<PaginatedSessionsResponse> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<PaginatedSessionsResponse>(`${API}/sessions`, { params });
  }

  /**
   * GET /api/Chat/sessions/{sessionId}/history
   * Returns the full session object + all messages. Returns 404 if not found.
   */
  getSessionHistory(sessionId: string): Observable<ChatHistoryResponse> {
    return this.http.get<ChatHistoryResponse>(`${API}/sessions/${sessionId}/history`);
  }

  /**
   * PUT /api/Chat/sessions/{sessionId}/end
   * Ends an active session and returns the updated SessionModel.
   */
  endSession(sessionId: string): Observable<SessionModel> {
    return this.http.put<SessionModel>(`${API}/sessions/${sessionId}/end`, {});
  }

  /**
   * DELETE /api/Chat/sessions/{sessionId}
   * Soft-deletes a session (204 NoContent on success).
   */
  deleteSession(sessionId: string): Observable<void> {
    return this.http.delete<void>(`${API}/sessions/${sessionId}`);
  }

  /**
   * PUT /api/Chat/sessions/{sessionId}/title
   * Updates the display title of a session. Returns the updated SessionModel.
   */
  updateSessionTitle(sessionId: string, title: string): Observable<SessionModel> {
    return this.http.put<SessionModel>(`${API}/sessions/${sessionId}/title`, { title });
  }

  // ── Message Endpoints ────────────────────────────────────────────────────────

  /**
   * POST /api/Chat/send
   * HTTP FALLBACK ONLY — use SignalR (SendMessage) as the primary channel.
   * Falls back to this when SignalR connection fails.
   */
  sendMessageHttp(dto: UserChatRequest): Observable<SendMessageResponse> {
    return this.http.post<SendMessageResponse>(`${API}/send`, dto);
  }
}
