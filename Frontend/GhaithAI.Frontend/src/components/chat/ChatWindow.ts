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
    .prompt-card {
      background-color: var(--background, #fff);
      color: var(--foreground, #1a1a1a);
      border: 1px solid var(--border, #e5e5e5);
      border-radius: 0.75rem;
      transition: all 0.2s ease;
      font-size: 0.9rem;
    }
    .prompt-card:hover {
      background-color: var(--chat-secondary, #f0fdfa);
      border-color: var(--chat-primary, #0d9488);
      color: var(--chat-primary, #0d9488);
      transform: translateY(-2px);
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
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

    <!-- ── Empty State / Suggested Prompts ─────────────────── -->
    @if (!isLoading && showSuggestedPrompts) {
      <div class="h-100 d-flex flex-column align-items-center justify-content-center text-center p-4">
        <div class="mb-4">
          <div class="rounded-circle d-flex align-items-center justify-content-center mx-auto" style="width: 80px; height: 80px; background-color: var(--chat-secondary, #f0fdfa);">
            <i class="bi bi-robot" style="font-size: 2.5rem; color: var(--chat-primary);"></i>
          </div>
        </div>
        <h2 class="h4 mb-3 fw-bold" style="color: var(--foreground);">How can I help you today?</h2>
        <p class="text-muted mb-4 max-w-md" style="max-width: 450px;">
          I'm here to listen and support you. You can share your feelings or choose a topic below to start a conversation.
        </p>
        
        <div class="row g-3 w-100" style="max-width: 600px;">
          @for (prompt of suggestedPrompts; track prompt) {
            <div class="col-12 col-md-6">
              <button
                type="button"
                class="btn w-100 h-100 p-3 text-start prompt-card shadow-sm d-flex align-items-center"
                (click)="promptSelected.emit(prompt)">
                <i class="bi bi-chat-left-text me-3" style="color: var(--chat-primary); opacity: 0.8;"></i>
                <span class="fw-medium">{{ prompt }}</span>
              </button>
            </div>
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
