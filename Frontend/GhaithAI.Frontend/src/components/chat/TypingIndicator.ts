// File: src/components/chat/TypingIndicator.ts

import {
  Component,
  Input,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-typing-indicator',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    @keyframes bounce {
      0%, 100% { transform: translateY(0);   opacity: 0.6; }
      50%       { transform: translateY(-6px); opacity: 1;   }
    }
    .dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      display: inline-block;
      background-color: var(--muted-foreground, #737373);
      animation: bounce 1.2s infinite ease-in-out;
    }
    .typing-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }
    .typing-bubble {
      border-radius: 1rem 1rem 1rem 0.25rem;
      padding: 1rem;
      display: flex;
      align-items: center;
      gap: 6px;
      background-color: var(--secondary, #f5f5f5);
    }
  `],
  template: `
    @if (visible) {
      <div class="d-flex gap-3 align-items-end" role="status" aria-label="GhaithAI is typing">

        <!-- AI avatar -->
        <div class="typing-avatar" style="background-color: color-mix(in srgb, var(--chat-primary) 15%, transparent);">
          <i class="bi bi-cpu small" style="color: var(--chat-primary, #0d9488);"></i>
        </div>

        <!-- Animated dots bubble -->
        <div class="typing-bubble">
          <span class="dot" style="animation-delay: 0ms;"></span>
          <span class="dot" style="animation-delay: 150ms;"></span>
          <span class="dot" style="animation-delay: 300ms;"></span>
        </div>

      </div>
    }
  `,
})
export class TypingIndicator {
  @Input() visible: boolean = false;
}
