// File: src/components/chat/ChatMessage.ts

import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  signal,
} from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ChatMessageModel } from '../../types/chat.types';

@Component({
  selector: 'app-chat-message',
  standalone: true,
  imports: [CommonModule, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(8px); }
      to   { opacity: 1; transform: translateY(0); }
    }
    .message-enter {
      animation: fadeIn 0.25s ease-out forwards;
    }

    /* ── Avatar ───────────────────────────────────────── */
    .msg-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    /* ── Bubble ───────────────────────────────────────── */
    .msg-bubble {
      position: relative;
      padding: 0.75rem 1rem;
      border-radius: 1rem;
      font-size: 0.875rem;
      line-height: 1.5;
      white-space: pre-wrap;
      word-break: break-word;
      transition: opacity 0.15s ease;
      max-width: 80%;
    }
    .msg-bubble.bubble-ai {
      border-radius: 1rem 1rem 1rem 0.25rem;
    }
    .msg-bubble.bubble-user {
      border-radius: 1rem 1rem 0.25rem 1rem;
    }

    /* ── Copy button ──────────────────────────────────── */
    .copy-btn {
      position: absolute;
      top: 0.5rem;
      right: 0.5rem;
      opacity: 0;
      padding: 0.25rem 0.375rem;
      border-radius: 0.375rem;
      background-color: rgba(0,0,0,0.05);
      border: none;
      transition: opacity 0.15s, background-color 0.15s;
      line-height: 1;
    }
    .msg-bubble:hover .copy-btn {
      opacity: 1;
    }
    .copy-btn:hover {
      background-color: rgba(0,0,0,0.1);
    }

    /* ── Timestamp ────────────────────────────────────── */
    .timestamp {
      font-size: 11px;
      opacity: 0;
      transition: opacity 0.2s ease;
    }
    .msg-bubble-wrapper:hover .timestamp {
      opacity: 1;
    }
  `],
  template: `
    <div class="message-enter d-flex gap-3 align-items-end w-100"
         [class.flex-row-reverse]="message.senderType === 'User'">

      <!-- Avatar -->
      <div class="msg-avatar shadow-sm border"
           [style.border-color]="message.senderType === 'AI' ? 'var(--chat-primary)' : 'transparent'"
           [style.background-color]="message.senderType === 'AI'
             ? 'var(--chat-secondary, #f0fdfa)'
             : 'var(--chat-primary, #0d9488)'">
        @if (message.senderType === 'AI') {
          <i class="bi bi-robot" style="color: var(--chat-primary, #0d9488);"></i>
        } @else {
          <i class="bi bi-person-fill" style="color: #ffffff;"></i>
        }
      </div>

      <!-- Bubble + meta wrapper -->
      <div class="msg-bubble-wrapper d-flex flex-column gap-1"
           [class.align-items-end]="message.senderType === 'User'"
           style="max-width: 80%;">

        <!-- Bubble -->
        <div class="msg-bubble shadow-sm"
             [class.bubble-ai]="message.senderType === 'AI'"
             [class.bubble-user]="message.senderType === 'User'"
             [class.opacity-50]="message.isOptimistic || message.status === 'sending'"
             [style.background-color]="message.senderType === 'AI' ? 'var(--chat-secondary, #f0fdfa)' : 'var(--chat-primary, #0d9488)'"
             [style.color]="message.senderType === 'AI' ? 'var(--foreground, #1a1a1a)' : '#ffffff'"
             [style.border]="message.status === 'error' ? '1.5px solid var(--destructive, #dc2626)' : (message.senderType === 'AI' ? '1px solid var(--border, #e5e5e5)' : '1px solid transparent')">

          {{ message.content }}

          <!-- Copy Button (AI only) -->
          @if (message.senderType === 'AI' && message.status !== 'error') {
            <button
              type="button"
              class="copy-btn"
              (click)="copyContent()"
              [title]="isCopied() ? 'Copied!' : 'Copy message'">
              @if (isCopied()) {
                <i class="bi bi-check2" style="color: #16a34a;"></i>
              } @else {
                <i class="bi bi-copy" style="color: var(--muted-foreground, #737373);"></i>
              }
            </button>
          }
        </div>

        <!-- Meta below bubble -->
        <div class="d-flex align-items-center gap-2 px-1">

          <!-- Error state -->
          @if (message.status === 'error') {
            <div class="d-flex align-items-center gap-1" style="color: var(--destructive, #dc2626); font-size: 0.75rem; font-weight: 600;">
              <i class="bi bi-exclamation-triangle-fill"></i>
              <span>Not sent</span>
              <button
                type="button"
                class="btn btn-link btn-sm p-0 ms-1 d-flex align-items-center gap-1"
                style="color: var(--destructive, #dc2626); font-size: 0.75rem; text-decoration: none;"
                (click)="retryMessage.emit(message)">
                <i class="bi bi-arrow-clockwise"></i> Retry
              </button>
            </div>
          }

          <!-- Timestamp (on hover) -->
          @if (message.status !== 'error') {
            <span class="timestamp" style="color: var(--muted-foreground, #737373);">
              {{ message.sentAt | date:'shortTime' }}
              @if (message.status === 'sending') {
                <i class="bi bi-clock ms-1"></i>
              }
              @if (message.status === 'sent' && message.senderType === 'User') {
                <i class="bi bi-check2 ms-1"></i>
              }
            </span>
          }

        </div>
      </div>
    </div>
  `,
})
export class ChatMessage {
  @Input() message!: ChatMessageModel;
  @Output() retryMessage = new EventEmitter<ChatMessageModel>();

  readonly isCopied = signal<boolean>(false);

  copyContent(): void {
    if (!this.message.content) return;
    navigator.clipboard.writeText(this.message.content).then(() => {
      this.isCopied.set(true);
      setTimeout(() => this.isCopied.set(false), 2000);
    }).catch(err => console.error('Failed to copy', err));
  }
}
