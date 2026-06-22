import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, timer } from 'rxjs';
import { switchMap, takeWhile, map, expand, delay } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export type TranscriptionStatus = 'Processing' | 'Completed' | 'Failed';

export interface TranscriptStatusResponse {
  status: TranscriptionStatus;
}

export interface TranscriptSegment {
  id: string;
  speaker: 'Doctor' | 'Patient';
  content: string;
  startMs: number;
  endMs: number;
  confidenceScore: number | null;
  isEdited: boolean;
}

@Injectable({ providedIn: 'root' })
export class TranscriptService {

  private readonly baseUrl = `${environment.apiUrl}/sessions`;

  constructor(private http: HttpClient) {}

  upload(sessionId: string, audioBlob: Blob): Observable<{ status: string }> {
    const formData = new FormData();
    formData.append('audioFile', audioBlob, `session-${sessionId}.webm`);

    return this.http.post<{ status: string }>(
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
   * Polling بـ exponential backoff: بيبدأ كل 5 ثوانٍ، يزود الفترة
   * تدريجياً (5s → 8s → 12s...) لحد ما status يبقى Completed أو Failed.
   * بيوقف نفسه تلقائياً — مش محتاج unsubscribe يدوي خارجي.
   */
  pollStatus(sessionId: string): Observable<TranscriptionStatus> {
    let currentDelay = 5000;
    const maxDelay = 15000;

    return timer(0).pipe(
      expand(() =>
        this.getStatus(sessionId).pipe(
          delay(0),
          switchMap((res) => {
            if (res.status === 'Processing') {
              currentDelay = Math.min(currentDelay * 1.4, maxDelay);
              return timer(currentDelay).pipe(map(() => res));
            }
            return timer(0).pipe(map(() => res));
          })
        )
      ),
      map((res: any) => (res.status ?? res) as TranscriptionStatus),
      takeWhile((status) => status === 'Processing', true)
    );
  }

  getSegments(sessionId: string): Observable<TranscriptSegment[]> {
    return this.http.get<TranscriptSegment[]>(`${this.baseUrl}/${sessionId}/transcript`);
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
