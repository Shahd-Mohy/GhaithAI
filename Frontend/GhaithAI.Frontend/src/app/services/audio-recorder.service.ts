import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AudioRecorderService {

  private recorder: MediaRecorder | null = null;
  private chunks: Blob[] = [];

  start(stream: MediaStream): void {
    this.chunks = [];

    this.recorder = new MediaRecorder(stream, { mimeType: 'audio/webm' });

    this.recorder.ondataavailable = (event) => {
      if (event.data.size > 0) {
        this.chunks.push(event.data);
      }
    };

    // timeslice كل دقيقة — بيمنع تراكم chunk واحد ضخم في الذاكرة
    this.recorder.start(60_000);
  }

  stop(): Promise<Blob> {
    return new Promise((resolve, reject) => {
      if (!this.recorder) {
        reject(new Error('Recorder was never started.'));
        return;
      }

      this.recorder.onstop = () => {
        const blob = new Blob(this.chunks, { type: 'audio/webm' });
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
}
