// File: src/components/chat/CrisisOverlay.ts

import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-crisis-overlay',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    @keyframes slideDown {
      from { opacity: 0; transform: translateY(-100%); }
      to   { opacity: 1; transform: translateY(0); }
    }
    .crisis-banner {
      animation: slideDown 0.3s ease-out forwards;
      background-color: rgba(220, 38, 38, 0.08);
      border-bottom: 1px solid rgba(220, 38, 38, 0.25);
      padding: 1rem 1.5rem;
      flex-shrink: 0;
      width: 100%;
    }
    .btn-help {
      background-color: var(--destructive, #dc2626);
      color: #fff;
      border: none;
      border-radius: 0.5rem;
      padding: 0.5rem 1rem;
      font-size: 0.875rem;
      font-weight: 500;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      transition: opacity 0.15s ease, transform 0.15s ease;
      text-decoration: none;
    }
    .btn-help:hover {
      opacity: 0.88;
      transform: scale(1.02);
      color: #fff;
    }
    .btn-safe {
      background-color: var(--background, #fff);
      color: var(--foreground, #1a1a1a);
      border: 1px solid var(--border, #e5e5e5);
      border-radius: 0.5rem;
      padding: 0.5rem 1rem;
      font-size: 0.875rem;
      font-weight: 500;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      transition: background-color 0.15s ease;
    }
    .btn-safe:hover {
      background-color: var(--accent, #f0f0f0);
      color: var(--accent-foreground, #1a1a1a);
    }
  `],
  template: `
    @if (visible) {
      <div class="crisis-banner"
           role="alert"
           aria-live="assertive">

        <div class="d-flex align-items-start gap-3">

          <!-- Warning icon -->
          <i class="bi bi-exclamation-triangle-fill fs-5 flex-shrink-0 mt-1"
             style="color: var(--destructive, #dc2626);"></i>

          <!-- Content -->
          <div class="flex-grow-1 min-w-0">
            <p class="mb-1 fw-semibold small lh-sm"
               style="color: var(--destructive, #dc2626);">
              I'm concerned about what you've shared
            </p>
            <p class="mb-0 small lh-base"
               style="color: var(--muted-foreground, #737373);">
              If you're having thoughts of hurting yourself, please reach out for
              immediate support. You don't have to face this alone.
            </p>

            @if (suggestedAction) {
              <p class="mb-0 small fst-italic mt-1"
                 style="color: var(--muted-foreground, #737373);">
                {{ suggestedAction }}
              </p>
            }

            <!-- Action buttons -->
            <div class="d-flex align-items-center flex-wrap gap-2 mt-3">
              <button
                type="button"
                class="btn-help"
                (click)="getCrisisHelp.emit()">
                <i class="bi bi-telephone-fill"></i>
                Get Help Now
              </button>
              <button
                type="button"
                class="btn-safe"
                (click)="dismissed.emit()">
                I'm safe
              </button>
            </div>
          </div>
        </div>

      </div>
    }
  `,
})
export class CrisisOverlay {
  @Input() visible: boolean = false;
  @Input() suggestedAction: string = '';

  @Output() getCrisisHelp = new EventEmitter<void>();
  @Output() dismissed     = new EventEmitter<void>();
}
