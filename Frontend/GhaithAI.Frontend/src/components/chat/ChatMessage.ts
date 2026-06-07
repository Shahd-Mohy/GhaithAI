// File: src/components/chat/ChatMessage.ts

import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  signal,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { interval, Subscription } from 'rxjs';
import { ChatMessageModel } from '../../types/chat.types';

@Component({
  selector: 'app-chat-message',
  standalone: true,
  imports: [CommonModule, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    /* ── Fluid Kinetic Animations (Phase 2) ────────────── */
    @keyframes fadeInUp {
      from { opacity: 0; transform: translateY(10px); }
      to   { opacity: 1; transform: translateY(0); }
    }
    
    .message-enter {
      animation: fadeInUp 0.35s cubic-bezier(0.4, 0, 0.2, 1) forwards;
      will-change: transform, opacity;
    }

    /* ── Avatar ───────────────────────────────────────── */
    .msg-avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      box-shadow: var(--shadow-sm);
    }

    /* ── Bubble Base ──────────────────────────────────── */
    .msg-bubble {
      position: relative;
      padding: 0.875rem 1.25rem;
      font-size: 0.95rem;
      line-height: 1.6;
      white-space: pre-wrap;
      word-break: break-word;
      transition: opacity 0.2s ease;
      width: fit-content;
      max-width: 100%;
      box-shadow: var(--shadow-sm);
    }

    /* ── AI Bubble (Mint-Teal Glass) ──────────────────── */
    .msg-bubble.bubble-ai {
      background-color: var(--chat-bubble-ai, #f0fdfa);
      color: var(--text-main, #0f172a);
      /* Asymmetric organic border-radius for AI */
      border-radius: 20px 20px 20px 4px;
      border: 1px solid var(--chat-bubble-ai-border, rgba(13, 148, 136, 0.12));
    }

    /* ── User Bubble (Solid Deep Teal) ────────────────── */
    .msg-bubble.bubble-user {
      background-color: var(--chat-bubble-user, #0d9488);
      color: #ffffff;
      /* Asymmetric organic border-radius for User */
      border-radius: 20px 20px 4px 20px;
      border: 1px solid transparent;
      /* Subtle inner glow for depth */
      box-shadow: inset 0 1px 1px rgba(255,255,255,0.15), var(--shadow-sm);
      align-self: flex-end;
    }

    /* ── Copy button ──────────────────────────────────── */
    .copy-btn {
      position: absolute;
      top: 0.5rem;
      right: 0.5rem;
      opacity: 0;
      padding: 0.25rem 0.375rem;
      border-radius: var(--radius-sm, 6px);
      background-color: rgba(0,0,0,0.04);
      border: none;
      transition: all 0.2s ease;
      line-height: 1;
      cursor: pointer;
    }
    .msg-bubble:hover .copy-btn {
      opacity: 1;
    }
    .copy-btn:hover {
      background-color: rgba(0,0,0,0.08);
      transform: scale(1.05);
    }

    /* ── Metadata & Timestamp ─────────────────────────── */
    .meta-row {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 0 4px;
      margin-top: 4px;
    }

    .timestamp {
      font-size: 0.7rem;
      font-weight: 500;
      color: var(--text-muted, #64748b);
      opacity: 0;
      transition: opacity 0.25s ease;
    }
    .msg-bubble-wrapper:hover .timestamp {
      opacity: 1;
    }

    .error-text {
      color: var(--destructive, #dc2626);
      font-size: 0.75rem;
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: 4px;
    }
    
    /* ── System Fallback Bubble (Empathetic Error) ── */
    .msg-bubble.bubble-system-error {
      background-color: rgba(13, 148, 136, 0.04);
      color: var(--text-main, #334155);
      border-radius: 1rem;
      border: 1px solid rgba(13, 148, 136, 0.15);
      box-shadow: 0 2px 8px rgba(0,0,0,0.02);
      font-size: 0.95rem;
      line-height: 1.6;
    }
    
    .btn-retry-empathetic {
      background: transparent;
      border: none;
      color: var(--chat-primary, #0d9488);
      border-radius: 99px;
      padding: 0.35rem 0.5rem;
      font-size: 0.85rem;
      font-weight: 500;
      cursor: pointer;
      opacity: 0.8;
      transition: opacity 0.2s ease, transform 0.2s ease;
      display: inline-flex;
      align-items: center;
    }
    .btn-retry-empathetic:hover {
      opacity: 1;
      transform: translateY(-1px);
    }
    
    .btn-retry {
      color: var(--destructive, #dc2626);
      font-size: 0.75rem;
      font-weight: 600;
      background: none;
      border: none;
      padding: 0;
      cursor: pointer;
      text-decoration: underline;
      margin-left: 4px;
    }
  `],
  template: `
    <div class="message-enter d-flex gap-3 align-items-end w-100"
         [class.flex-row-reverse]="message.senderType === 'User'">

      <!-- Avatar -->
      <div class="msg-avatar"
           [style.border]="message.senderType === 'AI' ? '1px solid var(--chat-primary-subtle)' : 'none'"
           [style.background-color]="message.senderType === 'AI'
             ? 'var(--chat-bubble-ai, #f0fdfa)'
             : 'var(--chat-bubble-user, #0d9488)'">
        @if (message.senderType === 'AI') {
          <i class="bi bi-robot" style="color: var(--chat-primary, #0d9488); font-size: 1rem;"></i>
        } @else {
          <i class="bi bi-person-fill" style="color: #ffffff; font-size: 1rem;"></i>
        }
      </div>

      <!-- Bubble + meta wrapper -->
      <div class="msg-bubble-wrapper d-flex flex-column"
           [class.align-items-end]="message.senderType === 'User'"
           style="max-width: 85%;">

        <!-- Bubble -->
        <div class="msg-bubble"
             [class.bubble-ai]="message.senderType === 'AI' && message.status !== 'error'"
             [class.bubble-user]="message.senderType === 'User'"
             [class.bubble-system-error]="message.senderType === 'AI' && message.status === 'error'"
             [class.opacity-50]="message.isOptimistic || message.status === 'sending'"
             [style.border-color]="message.senderType === 'User' && message.status === 'error' ? 'var(--destructive, #dc2626)' : ''">

          {{ displayedContent() }}

          @if (message.senderType === 'AI' && message.status === 'error') {
            <div class="mt-3">
              <button type="button" class="btn-retry-empathetic" (click)="retryMessage.emit(message)">
                <i class="bi bi-arrow-clockwise me-2"></i>Retry
              </button>
            </div>
          }

          <!-- Copy Button (AI only) -->
          @if (message.senderType === 'AI' && message.status !== 'error') {
            <button
              type="button"
              class="copy-btn"
              (click)="copyContent()"
              [title]="isCopied() ? 'Copied!' : 'Copy message'">
              @if (isCopied()) {
                <i class="bi bi-check2" style="color: var(--chat-primary, #0d9488);"></i>
              } @else {
                <i class="bi bi-copy" style="color: var(--text-muted, #64748b);"></i>
              }
            </button>
          }
        </div>

        <!-- Meta below bubble -->
        <div class="meta-row">
          <!-- Error state -->
          @if (message.status === 'error' && message.senderType === 'User') {
            <div class="error-text">
              <i class="bi bi-exclamation-circle-fill"></i>
              <span>Not sent</span>
              <button
                type="button"
                class="btn-retry"
                (click)="retryMessage.emit(message)">
                Retry
              </button>
            </div>
          }

          <!-- Timestamp (on hover) -->
          @if (message.status !== 'error') {
            <span class="timestamp">
              {{ message.sentAt | date:'shortTime' }}
              @if (message.status === 'sending') {
                <i class="bi bi-clock ms-1"></i>
              }
              @if (message.status === 'sent' && message.senderType === 'User') {
                <i class="bi bi-check2-all ms-1"></i>
              }
            </span>
          }
        </div>

      </div>
    </div>
  `,
})
export class ChatMessage implements OnInit, OnDestroy {
  @Input() message!: ChatMessageModel;
  @Output() retryMessage = new EventEmitter<ChatMessageModel>();

  readonly isCopied = signal<boolean>(false);
  readonly displayedContent = signal<string>('');
  
  isRiskDetectedInBubble: boolean = false;

  private streamSub?: Subscription;

  ngOnInit() {
    const cleanContent = this.extractCleanContent(this.message.content);

    if (this.message.senderType === 'AI') {
      const age = Date.now() - new Date(this.message.sentAt).getTime();
      if (age < 10000) {
        this.streamText(cleanContent);
      } else {
        this.displayedContent.set(cleanContent);
      }
    } else {
      this.displayedContent.set(cleanContent);
    }
  }

  private extractCleanContent(rawContent: string): string {
    if (!rawContent) return '';
    
    const trimmed = rawContent.trim();
    if (trimmed.startsWith('{') && trimmed.endsWith('}')) {
      try {
        const parsed = JSON.parse(trimmed);
        if (parsed && typeof parsed === 'object') {
          const aiResponse = parsed.AiResponse ?? parsed.aiResponse;
          if (aiResponse !== undefined) {
            this.isRiskDetectedInBubble = !!(parsed.IsRiskDetected ?? parsed.isRiskDetected);
            return aiResponse;
          }
        }
      } catch (e) {
      }
    }
    return rawContent;
  }

  streamText(fullText: string) {
    let index = 0;
    this.streamSub = interval(30).subscribe(() => {
      if (index < fullText.length) {
        index++;
        this.displayedContent.set(fullText.substring(0, index));
      } else {
        this.streamSub?.unsubscribe();
      }
    });
  }

  ngOnDestroy() {
    this.streamSub?.unsubscribe();
  }

  copyContent(): void {
    if (!this.message.content) return;
    navigator.clipboard.writeText(this.message.content).then(() => {
      this.isCopied.set(true);
      setTimeout(() => this.isCopied.set(false), 2000);
    }).catch(err => console.error('Failed to copy', err));
  }
}
