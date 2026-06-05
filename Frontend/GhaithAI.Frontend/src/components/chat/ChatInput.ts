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
    /* ── Outer wrapper ───────────────────────────────── */
    .input-wrapper {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    /* ── Input container ─────────────────────────────── */
    .input-container {
      display: flex;
      align-items: flex-end;
      gap: 0.5rem;
      padding: 0.75rem 1rem;
      border: 1px solid var(--border, #e5e5e5);
      border-radius: 0.75rem;
      background-color: var(--background, #fff);
      transition: box-shadow 0.15s ease;
    }
    .input-container:focus-within {
      box-shadow: 0 0 0 2px var(--ring, #a3a3a3);
    }

    /* ── Textarea ────────────────────────────────────── */
    textarea {
      flex: 1 1 0%;
      resize: none;
      background: transparent;
      outline: none;
      border: none;
      color: var(--foreground, #1a1a1a);
      font-size: 0.875rem;
      line-height: 1.5rem;
      min-height: 2.5rem;
      max-height: 7.5rem;
      overflow-y: auto;
      padding: 0;
      padding-top: 2px;
    }
    textarea::placeholder {
      color: var(--muted-foreground, #737373);
    }

    /* ── Send button ─────────────────────────────────── */
    .send-btn {
      width: 2.25rem;
      height: 2.25rem;
      border-radius: 50%;
      background-color: var(--primary, #1a1a1a);
      color: var(--primary-foreground, #f8f8f8);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      border: none;
      transition: transform 0.15s ease, opacity 0.15s ease;
      padding: 0;
    }
    .send-btn:hover:not(:disabled) {
      transform: scale(1.08);
    }
    .send-btn:disabled {
      opacity: 0.4;
      cursor: not-allowed;
    }

    /* ── Character counter ───────────────────────────── */
    .char-counter {
      display: flex;
      justify-content: flex-end;
      padding: 0 4px;
    }
  `],
  template: `
    <div class="input-wrapper">

      <!-- Input row -->
      <div class="input-container">
        <textarea
          #textareaRef
          [placeholder]="placeholder"
          [disabled]="disabled"
          [attr.maxlength]="maxChars"
          [value]="inputText()"
          (input)="onInput($event)"
          (keydown.enter)="onEnterKey($event)"
          rows="1"
          [style.opacity]="disabled ? '0.5' : '1'"
          [style.cursor]="disabled ? 'not-allowed' : 'text'"
          aria-label="Message input">
        </textarea>

        <button
          type="button"
          class="send-btn"
          [disabled]="!canSend()"
          (click)="sendMessage()"
          aria-label="Send message">
          <i class="bi bi-send-fill" style="font-size: 0.85rem;"></i>
        </button>
      </div>

      <!-- Character counter (shown near limit) -->
      @if (charCount() > warnThreshold) {
        <div class="char-counter">
          <span class="small"
                [style.color]="charCount() >= maxChars ? 'var(--destructive, #dc2626)' : 'var(--muted-foreground, #737373)'">
            {{ charCount() }} / {{ maxChars }}
          </span>
        </div>
      }

    </div>
  `,
})
export class ChatInput {
  @Input() disabled: boolean = false;
  @Input() placeholder: string = 'Type your message...';
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
    const el = event.target as HTMLTextAreaElement;
    this.inputText.set(el.value);
    this.autoResize(el);
  }

  onEnterKey(event: Event): void {
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
    }
  }

  private autoResize(el: HTMLTextAreaElement): void {
    el.style.height = 'auto';
    el.style.height = `${el.scrollHeight}px`;
  }
}
