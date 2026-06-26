import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AuthService } from './auth';
import { environment } from '../../environments/environment';

// ─── Response envelope shapes ────────────────────────────────────────────────

export interface JournalDTO {
  journalId: string;
  title: string | null;
  contentPreview: string;
  content: string;
  promptType: string;
  moodBefore: string;
  tags: string[];
  wordCount: number;
  date: string;           // "yyyy-MM-dd"
  createdAt: string;      // ISO datetime
  updatedAt: string | null;
}

export interface JournalCreatedDTO {
  journalId: string;
  createdAt: string;
  wordCount: number;
}

export interface GetAllResponse {
  success: boolean;
  data: JournalDTO[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface SingleResponse {
  data: JournalDTO;
  success: boolean;
  message: string;
  statusCode: number;
}

export interface CreateResponse {
  data: JournalCreatedDTO;
  success: boolean;
  message: string;
  statusCode: number;
}

export interface ActionResponse {
  success: boolean;
  message: string;
  statusCode: number;
}

// ─── Request body shapes ─────────────────────────────────────────────────────

export interface CreateJournalRequest {
  title?: string | null;
  content: string;
  promptType?: string;    // default "free"
  moodBefore?: string;
  tags?: string | null;   // comma-separated e.g. "work,stress"
}

export interface UpdateJournalRequest {
  title?: string | null;
  content?: string | null;
  tags?: string | null;   // comma-separated
}

// ─── Service ─────────────────────────────────────────────────────────────────

@Injectable({
  providedIn: 'root'
})
export class JournalService {

  private readonly baseUrl = `${environment.apiUrl}/Journal`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  // ─── Auth header helper ───────────────────────────────────────────────────

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    });
  }

  // ─── GET /api/Journal ─────────────────────────────────────────────────────
  // Returns paginated list. search is optional.

  getAll(search?: string, page = 1, pageSize = 20): Observable<GetAllResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http
      .get<GetAllResponse>(this.baseUrl, { headers: this.getHeaders(), params })
      .pipe(catchError(this.handleError));
  }

  // ─── GET /api/Journal/{id} ────────────────────────────────────────────────

  getById(journalId: string): Observable<SingleResponse> {
    return this.http
      .get<SingleResponse>(`${this.baseUrl}/${journalId}`, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  // ─── POST /api/Journal ────────────────────────────────────────────────────
  // Returns 201 with { data: { journalId, createdAt, wordCount }, ... }

  create(payload: CreateJournalRequest): Observable<CreateResponse> {
    return this.http
      .post<CreateResponse>(this.baseUrl, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  // ─── PUT /api/Journal/{id} ────────────────────────────────────────────────
  // Only title, content, tags are updatable per UpdateJournalDTO

  update(journalId: string, payload: UpdateJournalRequest): Observable<ActionResponse> {
    return this.http
      .put<ActionResponse>(`${this.baseUrl}/${journalId}`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  // ─── DELETE /api/Journal/{id} (soft delete) ───────────────────────────────

  delete(journalId: string): Observable<ActionResponse> {
    return this.http
      .delete<ActionResponse>(`${this.baseUrl}/${journalId}`, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  // ─── Error handler ────────────────────────────────────────────────────────

  private handleError(error: any): Observable<never> {
    let message = 'An unexpected error occurred.';
    if (error?.error?.message) {
      message = error.error.message;
    } else if (error?.message) {
      message = error.message;
    }
    return throwError(() => new Error(message));
  }
}
