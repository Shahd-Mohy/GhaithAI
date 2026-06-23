import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, timer, EMPTY } from 'rxjs';
import { switchMap, takeWhile, map, expand } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export type TranscriptionStatus = 'Processing' | 'Completed' | 'Failed' | 'NotStarted';

export interface TranscriptStatusResponse {
  sessionId: string;
  // IMPORTANT: must match the JSON key the backend sends.
  // Backend DTO property "Status" → serialized as "status" (camelCase).
  // Previously this was typed as "transcriptionStatus" which is why
  // res.status was always undefined and polling never stopped.
  status: TranscriptionStatus;
}

export interface TranscriptSegment {
  id: string;
  clinicalSessionId: string;
  speaker: 'Doctor' | 'Patient';
  content: string;
  startMs: number;
  endMs: number;
  confidenceScore: number | null;
  isEdited: boolean;
  editedAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class TranscriptService {

  private readonly baseUrl = `${environment.apiUrl}/sessions`;

  constructor(private http: HttpClient) {}

  upload(sessionId: string, audioBlob: Blob): Observable<{ message: string }> {
    const formData = new FormData();
    formData.append('audioFile', audioBlob, `session-${sessionId}.webm`);

    return this.http.post<{ message: string }>(
      `${this.baseUrl}/${sessionId}/transcript/upload`,
      formData
    );
  }

  getStatus(sessionId: string): Observable<TranscriptStatusResponse> {
    return this.http.get<TranscriptStatusResponse>(
      `${this.baseUrl}/${sessionId}/transcript/status`
    );
  }

  /**
   * Polls the transcription status with exponential backoff.
   *
   * How it works:
   * 1. Makes the first HTTP call immediately (no initial delay).
   * 2. If status is still "Processing", waits currentDelay ms, then calls again.
   * 3. The delay grows by 1.4x each time, capped at 15 seconds.
   * 4. Stops automatically when status becomes "Completed" or "Failed".
   *
   * The key fix here vs the old version:
   * - Old: started with timer(0) which emitted a number (0) first, causing
   *   map(res => res.status ?? res) to return 0, making takeWhile quit instantly.
   * - New: starts directly with getStatus() so the first emission is always
   *   a proper TranscriptStatusResponse object.
   */
  pollStatus(sessionId: string): Observable<TranscriptionStatus> {
    let currentDelay = 5000;
    const maxDelay = 15000;

    return this.getStatus(sessionId).pipe(
      expand((res) => {
        if (res.status === 'Processing' || res.status === 'NotStarted') {
          currentDelay = Math.min(currentDelay * 1.4, maxDelay);
          // Wait currentDelay ms, then fire the next HTTP call
          return timer(currentDelay).pipe(
            switchMap(() => this.getStatus(sessionId))
          );
        }
        // Status is Completed or Failed — stop expanding
        return EMPTY;
      }),
      map((res) => res.status),
      takeWhile((status) => status === 'Processing' || status === 'NotStarted', true)
    );
  }

  getSegments(sessionId: string): Observable<TranscriptSegment[]> {
    return this.http.get<TranscriptSegment[]>(
      `${this.baseUrl}/${sessionId}/transcript`
    );
  }

  updateSegment(
    sessionId: string,
    segmentId: string,
    content: string
  ): Observable<TranscriptSegment> {
    return this.http.put<TranscriptSegment>(
      `${this.baseUrl}/${sessionId}/transcript/${segmentId}`,
      { content }
    );
  }
}