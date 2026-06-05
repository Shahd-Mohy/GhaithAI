// File: src/store/crisis.store.ts

import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class CrisisStore {
  // ── Signals ──────────────────────────────────────────────────────────────────
  readonly showCrisisOverlay = signal<boolean>(false);
  readonly crisisMessage     = signal<string>('');

  // ── Mutators ─────────────────────────────────────────────────────────────────

  /**
   * Trigger the crisis overlay with an optional message.
   * Falls back to a generic distress message if none is provided.
   */
  trigger(message?: string): void {
    this.crisisMessage.set(
      message ?? 'We noticed signs of distress. You are not alone — please seek support.'
    );
    this.showCrisisOverlay.set(true);
  }

  /** Hide the crisis overlay (user dismissed or confirmed safety). */
  dismiss(): void {
    this.showCrisisOverlay.set(false);
  }
}
