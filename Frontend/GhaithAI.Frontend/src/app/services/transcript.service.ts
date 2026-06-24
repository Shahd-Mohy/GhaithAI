import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, timer, EMPTY } from 'rxjs';
import { switchMap, takeWhile, map, expand } from 'rxjs/operators';
import { environment } from '../../environments/environment';

// ── TranscriptionStatus ───────────────────────────────────────────────────────
// These are the exact string values the backend enum serializes to.
// The backend has a C# enum: TranscriptionStatus { NotStarted, Processing, Completed, Failed }
// With JsonStringEnumConverter registered in Program.cs, C# sends the name as a string.
// ──────────────────────────────────────────────────────────────────────────────
export type TranscriptionStatus = 'Processing' | 'Completed' | 'Failed' | 'NotStarted';

// ── TranscriptStatusResponse ──────────────────────────────────────────────────
// This interface MUST match exactly what the backend sends in JSON.
// Backend C# DTO property name → JSON key (ASP.NET Core uses camelCase by default):
//   "SessionId"  → "sessionId"
//   "Status"     → "status"
//   "ErrorDetail" → "errorDetail"   (NEW — optional, see below)
//
// CRITICAL: The old DTO had a property named "TranscriptionStatus" which serialized
// to "transcriptionStatus" in JSON. The frontend was reading "res.status" (undefined),
// so the polling never detected "Completed". We renamed the C# property to "Status"
// so the JSON key is "status" and res.status works correctly.
//
// errorDetail is OPTIONAL on purpose. If the backend doesn't send it (older
// API version, or it genuinely has nothing more specific to say), `res.errorDetail`
// is simply `undefined` and the UI falls back to the generic "Transcription
// failed" message — nothing breaks either way. If you add this field on the
// backend, populate it with whatever the speech vendor (Azure/AssemblyAI)
// reported, e.g. "Unsupported audio codec" or "Audio file too short".
// ──────────────────────────────────────────────────────────────────────────────
export interface TranscriptStatusResponse {
  sessionId: string;
  status: TranscriptionStatus;
  errorDetail?: string | null;
}

// ── TranscriptSegment ─────────────────────────────────────────────────────────
// One utterance returned by AssemblyAI, stored in the SessionTranscripts table,
// and returned by GET /api/sessions/{sessionId}/transcript.
// All field names are camelCase to match ASP.NET Core's default JSON serialization.
// ──────────────────────────────────────────────────────────────────────────────
export interface TranscriptSegment {
  id: string;
  clinicalSessionId: string;
  speaker: 'Doctor' | 'Patient';
  content: string;
  startMs: number;    // milliseconds from start of recording
  endMs: number;      // milliseconds from start of recording
  confidenceScore: number | null;
  isEdited: boolean;
  editedAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class TranscriptService {

  // All transcript endpoints live under /api/sessions/{sessionId}/transcript/...
  private readonly baseUrl = `${environment.apiUrl}/sessions`;

  constructor(private http: HttpClient) {}

  // ── upload() ──────────────────────────────────────────────────────────────────
  // Sends the recorded audio blob to the backend as multipart/form-data.
  // The backend field name must be "audioFile" — that's what [FromForm] UploadAudioDto
  // expects on the C# side.
  // Returns 202 Accepted (not 200) — meaning "received, processing in background".
  //
  // CHANGED: filename extension now comes from the blob's own `type` instead of
  // being hardcoded as ".webm". MediaRecorder may have used a different codec
  // than expected (see audio-recorder.service.ts) — if the bytes and the
  // filename disagree about the format, some backends/vendors misidentify the
  // container and fail to decode it. Passing the real MIME type keeps them in sync.
  // ──────────────────────────────────────────────────────────────────────────────
  upload(sessionId: string, audioBlob: Blob): Observable<{ message: string }> {
    const formData = new FormData();
    const extension = this.extensionFromMimeType(audioBlob.type);
    formData.append('audioFile', audioBlob, `session-${sessionId}.${extension}`);

    return this.http.post<{ message: string }>(
      `${this.baseUrl}/${sessionId}/transcript/upload`,
      formData
    );
  }

  // Maps a MIME type like 'audio/webm;codecs=opus' to a plain file extension.
  // Falls back to 'webm' if the type is missing or unrecognized.
  private extensionFromMimeType(mimeType: string): string {
    if (!mimeType) return 'webm';
    if (mimeType.includes('webm')) return 'webm';
    if (mimeType.includes('ogg')) return 'ogg';
    if (mimeType.includes('mp4') || mimeType.includes('m4a')) return 'm4a';
    if (mimeType.includes('wav')) return 'wav';
    return 'webm';
  }

  // ── getStatus() ───────────────────────────────────────────────────────────────
  // Single status check. Returns { sessionId, status, errorDetail? } where
  // status is one of NotStarted / Processing / Completed / Failed.
  // ──────────────────────────────────────────────────────────────────────────────
  getStatus(sessionId: string): Observable<TranscriptStatusResponse> {
    return this.http.get<TranscriptStatusResponse>(
      `${this.baseUrl}/${sessionId}/transcript/status`
    );
  }

  // ── pollStatus() ──────────────────────────────────────────────────────────────
  // This is the key method that makes the Processing → Reviewable transition work.
  //
  // HOW IT WORKS:
  //   1. Makes the first HTTP call immediately (no initial wait).
  //   2. If status is still "Processing" or "NotStarted", waits currentDelay ms,
  //      then calls again. The delay grows by 1.4× each time, capped at 15 seconds.
  //      Timeline: 5s → 7s → 10s → 14s → 15s → 15s → ...
  //   3. When status becomes "Completed" or "Failed", the Observable completes
  //      automatically — no manual cleanup needed.
  //
  // THE RxJS OPERATORS USED:
  //   expand()     — like flatMap but recursive: takes each emission and produces
  //                  the next Observable from it. Perfect for "call, check, wait, repeat".
  //   EMPTY        — a special Observable that completes immediately without emitting
  //                  anything. Returning EMPTY from expand() stops the recursion.
  //   timer(ms)    — emits once after a delay. We use it to wait between polls.
  //   switchMap()  — cancels the previous inner Observable and switches to a new one.
  //                  Here it cancels the timer and makes a fresh HTTP call.
  //   map()        — transforms TranscriptStatusResponse → just the status string.
  //   takeWhile()  — keeps the Observable alive while condition is true.
  //                  The second argument `true` means "also emit the value that
  //                  caused the condition to fail" (so "Completed" gets emitted
  //                  before the Observable closes, and the component can react to it).
  //
  // CHANGED: now emits the full TranscriptStatusResponse (not just the status
  // string) so the component can read errorDetail when status is "Failed".
  // ──────────────────────────────────────────────────────────────────────────────
  pollStatus(sessionId: string): Observable<TranscriptStatusResponse> {
    let currentDelay = 5000;     // start at 5 seconds
    const maxDelay = 15000;      // never exceed 15 seconds between polls

    return this.getStatus(sessionId).pipe(

      expand((res) => {
        // res is a TranscriptStatusResponse object — always, because we start
        // with getStatus() not timer(). This is the fix for the old version.

        if (res.status === 'Processing' || res.status === 'NotStarted') {
          // Grow the delay for the next poll (exponential backoff).
          currentDelay = Math.min(currentDelay * 1.4, maxDelay);

          // Wait currentDelay ms, then fire the next HTTP call.
          // timer(currentDelay) emits once after the delay.
          // switchMap cancels the timer if the component unsubscribes early.
          return timer(currentDelay).pipe(
            switchMap(() => this.getStatus(sessionId))
          );
        }

        // Status is "Completed" or "Failed" — stop polling.
        // Returning EMPTY tells expand() "don't recurse anymore".
        return EMPTY;
      }),

      // Keep emitting while status is still "Processing" or "NotStarted".
      // The `true` argument means: also emit the final value (Completed/Failed)
      // that caused takeWhile to stop, so the component can react to it.
      takeWhile(
        (res) => res.status === 'Processing' || res.status === 'NotStarted',
        true
      )
    );
  }

  // ── getSegments() ─────────────────────────────────────────────────────────────
  // Returns all transcript segments for a session, sorted by startMs on the
  // frontend (the backend returns them in insertion order, not necessarily sorted).
  // ──────────────────────────────────────────────────────────────────────────────
  getSegments(sessionId: string): Observable<TranscriptSegment[]> {
    return this.http.get<TranscriptSegment[]>(
      `${this.baseUrl}/${sessionId}/transcript`
    );
  }

  // ── updateSegment() ───────────────────────────────────────────────────────────
  // Sends the doctor's corrected text for one segment.
  // PUT /api/sessions/{sessionId}/transcript/{segmentId}
  // Body: { content: "corrected text" }
  // Returns the updated segment so the viewer can refresh in place.
  // ──────────────────────────────────────────────────────────────────────────────
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