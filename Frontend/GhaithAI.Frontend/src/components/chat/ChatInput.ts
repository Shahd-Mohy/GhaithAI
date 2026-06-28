// File: src/components/chat/ChatInput.ts

import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  signal,
  computed,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

const MAX_CHARS = 2000;
const WARN_THRESHOLD = 1800;

@Component({
  selector: 'app-chat-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    /* ── Floating Command Deck (Phase 2) ──────────────────────── */
    .floating-deck-wrapper {
      position: absolute;
      bottom: 0;
      left: 0;
      right: 0;
      padding: 0 1rem 0.75rem 1rem;
      background: linear-gradient(to top, rgba(255,255,255,1) 20%, rgba(255,255,255,0) 100%);
      pointer-events: none; /* Let clicks pass through the gradient */
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .floating-deck {
      pointer-events: auto; /* Re-enable clicks for the deck */
      width: 100%;
      max-width: 800px;
      background: var(--bg-glass, rgba(255, 255, 255, 0.75));
      backdrop-filter: blur(16px) saturate(180%);
      -webkit-backdrop-filter: blur(16px) saturate(180%);
      border: 1px solid var(--border-subtle, rgba(226, 232, 240, 0.8));
      border-radius: 99px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.04);
      display: flex;
      flex-direction: row;
      align-items: center;
      transition: box-shadow 0.3s ease, border-color 0.3s ease;
      padding: 0.25rem 0.5rem 0.25rem 1.25rem;
    }

    .floating-deck:focus-within {
      border-color: var(--chat-primary, #0d9488);
      box-shadow: 0 0 0 3px var(--chat-primary-glow, rgba(13, 148, 136, 0.15));
    }

    /* ── Input Box Layer ─────────────────────────────── */
    .input-row {
      display: flex;
      align-items: center;
      width: 100%;
      gap: 0.75rem;
    }

    textarea {
      flex: 1 1 0%;
      resize: none;
      background: transparent;
      outline: none;
      border: none;
      color: var(--text-main, #0f172a);
      font-size: 0.95rem;
      line-height: 1.5;
      min-height: 1.5rem;
      max-height: 80px;
      overflow-y: auto;
      padding: 0.5rem 0;
      scrollbar-width: none; /* Hide scrollbar for cleaner look */
    }
    textarea::-webkit-scrollbar { display: none; }
    textarea::placeholder {
      color: var(--text-muted, #64748b);
      font-weight: 400;
    }

    /* ── Send Button (Absolute Perfect Circle) ───────── */
    .send-btn {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background-color: var(--chat-primary, #0d9488);
      color: #ffffff;
      border: none;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      cursor: pointer;
      transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
      outline: none;
    }
    
    .send-btn i {
      font-size: 1.15rem;
      transition: transform 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    }

    .send-btn:hover:not(:disabled) {
      background-color: var(--chat-primary-dark, #0f766e);
      transform: translateY(-1px);
    }
    
    .send-btn:hover:not(:disabled) i {
      transform: scale(1.05) translateY(-1px);
    }

    .send-btn:disabled {
      background-color: var(--border-light, #e2e8f0);
      color: var(--text-subtle, #94a3b8);
      cursor: not-allowed;
    }

    .char-counter {
      font-size: 0.7rem;
      font-weight: 500;
      font-family: monospace;
      padding-left: 1rem;
      flex-shrink: 0;
    }

    .page-footer-zone {
      width: 100%;
      max-width: 800px;
      margin: 0 auto;
      text-align: center;
      margin-top: 10px;
      font-size: 11px;
      color: var(--text-muted, #64748b);
      opacity: 0.65;
      letter-spacing: 0.01em;
      pointer-events: auto;
    }

    @media (max-width: 576px) {
      .floating-deck-wrapper {
        padding: 0 0.875rem calc(0.625rem + env(safe-area-inset-bottom));
        background: linear-gradient(to top, #ffffff 72%, rgba(255,255,255,0));
      }

      .floating-deck {
        border-radius: 24px;
        padding: 0.25rem 0.375rem 0.25rem 1rem;
        min-height: 48px;
      }

      .input-row {
        gap: 0.5rem;
      }

      textarea {
        min-width: 0;
        font-size: 0.92rem;
        line-height: 1.4;
        min-height: 1.35rem;
        max-height: 92px;
        padding: 0.55rem 0;
      }

      .send-btn {
        width: 36px;
        height: 36px;
      }

      .page-footer-zone {
        max-width: 330px;
        margin-top: 0.5rem;
        padding: 0 0.25rem;
        font-size: 0.68rem;
        line-height: 1.35;
      }
    }

    @media (max-width: 360px) {
      .floating-deck-wrapper {
        padding-inline: 0.625rem;
      }

      .floating-deck {
        padding-left: 0.875rem;
      }

      textarea {
        font-size: 0.875rem;
      }
    }
  `],
  template: `
    <div class="floating-deck-wrapper">
      <div class="floating-deck">
        
        <div class="input-row">
          <textarea
            #textareaRef
            [placeholder]="placeholder"
            [disabled]="disabled"
            [attr.maxlength]="maxChars"
            [value]="inputText()"
            (input)="onInput($event)"
            (keydown.enter)="onEnterKey($event)"
            rows="1"
            [style.opacity]="disabled ? '0.6' : '1'"
            [style.cursor]="disabled ? 'not-allowed' : 'text'"
            aria-label="Message input">
          </textarea>

          @if (charCount() > warnThreshold) {
            <div class="char-counter"
                 [style.color]="charCount() >= maxChars ? 'var(--destructive, #dc2626)' : 'var(--text-muted, #64748b)'">
              {{ charCount() }}/{{ maxChars }}
            </div>
          }

          <button
            type="button"
            class="send-btn"
            [disabled]="!canSend()"
            (click)="sendMessage()"
            aria-label="Send message">
            @if (disabled) {
              <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            } @else {
              <i class="bi bi-send-fill"></i>
            }
          </button>
        </div>
      </div>

      <!-- Decoupled Footer -->
      <div class="page-footer-zone">
        GhaithAI is an AI companion and does not provide medical advice.  
        If in crisis, use the <strong style="color: var(--destructive); font-weight: 600;">Crisis Support</strong> button.
      </div>
    </div>
  `,
})
export class ChatInput {
  @Input() disabled: boolean = false;
  @Input() placeholder: string = 'Message GhaithAI...';
  @Output() messageSent = new EventEmitter<string>();

  @ViewChild('textareaRef') private textareaRef!: ElementRef<HTMLTextAreaElement>;

  readonly maxChars      = MAX_CHARS;
  readonly warnThreshold = WARN_THRESHOLD;

  readonly inputText = signal<string>('');
  readonly charCount = computed(() => this.inputText().length);
  readonly canSend   = computed(
    () => this.inputText().trim().length > 0 && !this.disabled
  );

  onInput(event: Event): void {
    if(this.disabled) return;
    const el = event.target as HTMLTextAreaElement;
    this.inputText.set(el.value);
    this.autoResize(el);
  }

  onEnterKey(event: Event): void {
    if (this.disabled) return;
    const ke = event as KeyboardEvent;
    if (ke.shiftKey) return;
    ke.preventDefault();
    this.sendMessage();
  }

  sendMessage(): void {
    if (!this.canSend()) return;
    const text = this.inputText().trim();
    this.messageSent.emit(text);
    this.clearInput();
  }

  private clearInput(): void {
    this.inputText.set('');
    if (this.textareaRef?.nativeElement) {
      const el = this.textareaRef.nativeElement;
      el.value = '';
      el.style.height = 'auto';
      el.style.height = '44px';
    }
  }

  private autoResize(el: HTMLTextAreaElement): void {
    el.style.height = '44px';
    el.style.height = 'auto';
    el.style.height = `${el.scrollHeight}px`;
  }
}
