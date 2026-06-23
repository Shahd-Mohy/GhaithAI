import { Injectable } from '@angular/core';

// ── Why this file changed ──────────────────────────────────────────────────────
// THE BUG:
//   The old code did:  new MediaRecorder(stream, { mimeType: 'audio/webm' })
//   'audio/webm' with no codec is ambiguous. Depending on the browser/OS,
//   MediaRecorder may silently choose a codec that Azure/AssemblyAI's decoder
//   doesn't expect inside a .webm container. The recorder doesn't throw an
//   error — it happily produces a Blob — but the speech-to-text vendor then
//   fails to decode it server-side, which is why TranscriptionStatus ended up
//   "Failed" even though the upload itself succeeded (202 Accepted).
//
// THE FIX:
//   Explicitly request 'audio/webm;codecs=opus' (the codec every browser that
//   supports MediaRecorder for audio also supports, and the one Azure Speech /
//   AssemblyAI expect for webm). We check isTypeSupported() first and fall
//   back gracefully instead of assuming. We also expose the *actual* mimeType
//   the browser used (via the `mimeType` getter) so the upload step can stay
//   in sync — never hardcode the extension/content-type separately from what
//   the recorder actually produced.
// ──────────────────────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class AudioRecorderService {

  private recorder: MediaRecorder | null = null;
  private chunks: Blob[] = [];

  // The codec-qualified MIME type actually used by the current/last recording.
  // Read this AFTER calling start() — don't assume 'audio/webm;codecs=opus'
  // was used, in case the browser didn't support it and we fell back.
  private activeMimeType = 'audio/webm';

  start(stream: MediaStream): void {
    this.chunks = [];

    // ── Pick the most specific supported MIME type ───────────────────────────
    // Order matters: try the codec-qualified ones first, fall back to plain
    // 'audio/webm' only if nothing else is supported (very old browsers).
    const candidates = [
      'audio/webm;codecs=opus',
      'audio/webm',
      'audio/ogg;codecs=opus'
    ];

    const supported = candidates.find(type => MediaRecorder.isTypeSupported(type));

    if (!supported) {
      // Extremely unlikely in any Chromium/Firefox-based browser, but fail
      // loudly instead of silently recording in an unknown format.
      throw new Error('No supported audio recording MIME type found in this browser.');
    }

    this.activeMimeType = supported;
    this.recorder = new MediaRecorder(stream, { mimeType: supported });

    this.recorder.ondataavailable = (event) => {
      if (event.data.size > 0) {
        this.chunks.push(event.data);
      }
    };

    // timeslice كل دقيقة — بيمنع تراكم chunk واحد ضخم في الذاكرة
    this.recorder.start(60_000);
  }

  stop(): Promise<Blob> {
    return new Promise((resolve) => {
      if (!this.recorder) {
        // recorder was never started — return empty blob instead of hanging
        resolve(new Blob([], { type: this.activeMimeType }));
        return;
      }

      const mimeType = this.activeMimeType;

      const timeout = setTimeout(() => {
        resolve(new Blob(this.chunks, { type: mimeType }));
      }, 3000); // 3s safety net

      this.recorder.onstop = () => {
        clearTimeout(timeout);
        const blob = new Blob(this.chunks, { type: mimeType });
        this.chunks = [];
        this.recorder = null;
        resolve(blob);
      };

      this.recorder.stop();
    });
  }

  get isRecording(): boolean {
    return this.recorder?.state === 'recording';
  }

  // ── mimeType ───────────────────────────────────────────────────────────────
  // Exposes the exact MIME type used for the current/last recording, including
  // the codec. The upload step (transcript.service.ts) should use this to pick
  // the file extension instead of hardcoding '.webm', so the filename and the
  // actual encoded bytes never disagree.
  get mimeType(): string {
    return this.activeMimeType;
  }
}