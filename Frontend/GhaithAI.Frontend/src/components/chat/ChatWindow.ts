// File: src/components/chat/ChatWindow.ts

import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  ViewChild,
  ElementRef,
  AfterViewChecked,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatMessageModel } from '../../types/chat.types';
import { ChatMessage } from './ChatMessage';
import { TypingIndicator } from './TypingIndicator';

const NEAR_BOTTOM_THRESHOLD = 120;

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, ChatMessage, TypingIndicator],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      flex: 1 1 0%;
      height: 100%;
      overflow: hidden;
    }

    /* ── Scrollable messages ─────────────────────────────────── */
    .scroll-area {
      flex: 1 1 0%;
      overflow-y: auto;
      min-height: 0;
      padding: 1rem 1rem;
      scrollbar-width: thin;
      scrollbar-color: var(--border, #e5e5e5) transparent;
    }
    @media (min-width: 768px) {
      .scroll-area { padding: 1rem 1.5rem; }
    }
    .scroll-area::-webkit-scrollbar       { width: 5px; }
    .scroll-area::-webkit-scrollbar-thumb { background: var(--border, #e5e5e5); border-radius: 3px; }

    /* ── Message spacing ─────────────────────────────────────── */
    .message-list > * + * {
      margin-top: 1.5rem;
    }

    /* ── Skeleton ────────────────────────────────────────────── */
    .skeleton-bubble {
      background-color: var(--muted, #f5f5f5);
      animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
      border-radius: 1rem;
    }
    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50%       { opacity: .5; }
    }
    .skeleton-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      flex-shrink: 0;
    }

    /* ── Suggested prompts ───────────────────────────────────── */
    .prompts-bar {
      flex-shrink: 0;
      padding: 0 1rem 0.75rem;
    }
    @media (min-width: 768px) {
      .prompts-bar { padding: 0 1.5rem 0.75rem; }
    }
    .prompt-pill {
      background-color: var(--muted, #f5f5f5);
      color: var(--muted-foreground, #737373);
      border: 1px solid var(--border, #e5e5e5);
      border-radius: 999px;
      padding: 0.25rem 0.75rem;
      font-size: 0.75rem;
      transition: background-color 0.15s ease, transform 0.1s ease;
      white-space: nowrap;
    }
    .prompt-pill:hover {
      background-color: var(--secondary, #f0f0f0);
      color: var(--secondary-foreground, #1a1a1a);
      transform: translateY(-1px);
    }
  `],
  template: `
    <!-- ── Scrollable Messages Area ─────────────────────────── -->
    <div #scrollContainer class="scroll-area" (scroll)="onScroll()">
      <div class="message-list">

        <!-- Loading Skeletons -->
        @if (isLoading) {
          <!-- AI skeleton -->
          <div class="d-flex gap-3 align-items-end">
            <div class="skeleton-avatar skeleton-bubble"></div>
            <div class="skeleton-bubble" style="height:64px; width:60%;"></div>
          </div>
          <!-- User skeleton -->
          <div class="d-flex gap-3 align-items-end flex-row-reverse mt-4">
            <div class="skeleton-avatar skeleton-bubble"></div>
            <div class="skeleton-bubble" style="height:48px; width:40%;"></div>
          </div>
          <!-- AI skeleton -->
          <div class="d-flex gap-3 align-items-end mt-4">
            <div class="skeleton-avatar skeleton-bubble"></div>
            <div class="skeleton-bubble" style="height:96px; width:75%;"></div>
          </div>
        }

        <!-- Message list -->
        @if (!isLoading) {
          @for (msg of messages; track msg.id) {
            <app-chat-message
              [message]="msg"
              (retryMessage)="retryMessage.emit($event)" />
          }

          <!-- Typing indicator -->
          <app-typing-indicator [visible]="isAiTyping" />
        }

        <!-- Bottom scroll anchor -->
        <div style="height: 8px;"></div>
      </div>
    </div>

    <!-- ── Suggested Prompts ─────────────────────────────────── -->
    @if (!isLoading && showSuggestedPrompts && suggestedPrompts.length > 0) {
      <div class="prompts-bar">
        <p class="mb-2" style="font-size: 0.75rem; color: var(--muted-foreground);">
          Not sure where to start? Try one of these:
        </p>
        <div class="d-flex flex-wrap gap-2">
          @for (prompt of suggestedPrompts; track prompt) {
            <button
              type="button"
              class="prompt-pill btn p-0"
              (click)="promptSelected.emit(prompt)">
              {{ prompt }}
            </button>
          }
        </div>
      </div>
    }
  `,
})
export class ChatWindow implements AfterViewChecked {
  @Input() messages: ChatMessageModel[]  = [];
  @Input() isAiTyping: boolean           = false;
  @Input() isLoading: boolean            = false;
  @Input() suggestedPrompts: string[]    = [];
  @Input() showSuggestedPrompts: boolean = false;

  @Output() promptSelected = new EventEmitter<string>();
  @Output() retryMessage   = new EventEmitter<ChatMessageModel>();

  @ViewChild('scrollContainer') private scrollContainer!: ElementRef<HTMLDivElement>;

  private readonly shouldAutoScroll = signal<boolean>(true);

  ngAfterViewChecked(): void {
    if (this.shouldAutoScroll()) {
      this.scrollToBottom();
    }
  }

  onScroll(): void {
    this.shouldAutoScroll.set(this.isNearBottom());
  }

  private isNearBottom(): boolean {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return true;
    return el.scrollHeight - el.scrollTop - el.clientHeight < NEAR_BOTTOM_THRESHOLD;
  }

  private scrollToBottom(): void {
    try {
      const el = this.scrollContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
    } catch {
      // noop
    }
  }
}
