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
      transition: all 0.2s ease;
      background-color: var(--background, #fff);
    }
    .input-container:focus-within {
      border-color: var(--chat-primary, #0d9488) !important;
      box-shadow: 0 0 0 0.25rem rgba(13, 148, 136, 0.25) !important;
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
      transition: transform 0.15s ease, background-color 0.15s ease;
    }
    .send-btn:hover:not(:disabled) {
      transform: scale(1.05);
      background-color: var(--chat-primary-hover, #0f766e) !important;
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
    <div class="input-wrapper px-3 pb-3 pt-2 w-100 mx-auto" style="max-width: 900px;">

      <!-- Input row -->
      <div class="input-container d-flex align-items-end gap-2 p-2 border rounded-4 shadow-sm position-relative">
        <textarea
          #textareaRef
          class="form-control border-0 shadow-none bg-transparent resize-none py-2"
          [placeholder]="placeholder"
          [disabled]="disabled"
          [attr.maxlength]="maxChars"
          [value]="inputText()"
          (input)="onInput($event)"
          (keydown.enter)="onEnterKey($event)"
          rows="1"
          style="min-height: 44px; max-height: 120px;"
          [style.opacity]="disabled ? '0.5' : '1'"
          [style.cursor]="disabled ? 'not-allowed' : 'text'"
          aria-label="Message input">
        </textarea>

        <button
          type="button"
          class="send-btn btn rounded-circle d-flex align-items-center justify-content-center p-0 flex-shrink-0 mb-1 me-1"
          style="width: 36px; height: 36px; background-color: var(--chat-primary, #0d9488); color: white;"
          [disabled]="!canSend()"
          (click)="sendMessage()"
          aria-label="Send message">
          <i class="bi bi-arrow-up-short fs-4"></i>
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

      <!-- Medical Disclaimer -->
      <div class="text-center mt-2 px-3">
        <small class="text-muted d-block" style="font-size: 0.75rem;">
          GhaithAI is an AI companion and does not provide professional medical advice, diagnosis, or treatment. 
          If you are in a crisis, please use the <strong class="text-danger">Crisis Support</strong> button.
        </small>
      </div>

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
