// File: src/components/chat/ChatWindow.ts

import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  ViewChild,
  ElementRef,
  AfterViewInit,
  OnDestroy,
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
      position: relative;
    }

    /* ── Scrollable messages ─────────────────────────────────── */
    .scroll-area {
      flex: 1 1 0%;
      overflow-y: scroll; /* Force track to prevent layout width shift blink */
      overflow-anchor: none;
      transform: translateZ(0); /* Hardware acceleration */
      min-height: 0;
      padding: 1rem 1rem 0 1rem;
      scrollbar-width: thin;
      scrollbar-color: rgba(13, 148, 136, 0.2) transparent;
    }
    
    /* Custom Scrollbar to prevent flashing scroll tracks */
    .scroll-area::-webkit-scrollbar {
      width: 6px;
    }
    .scroll-area::-webkit-scrollbar-track {
      background: transparent;
    }
    .scroll-area::-webkit-scrollbar-thumb {
      background-color: rgba(13, 148, 136, 0.2);
      border-radius: 10px;
    }
    .scroll-area::-webkit-scrollbar-thumb:hover {
      background-color: rgba(13, 148, 136, 0.4);
    }

    @media (min-width: 768px) {
      .scroll-area { padding: 1.5rem 1.5rem 0 1.5rem; }
    }

    /* ── Message spacing ─────────────────────────────────────── */
    .message-list > * + * {
      margin-top: 1.5rem;
    }

    /* ── Skeleton ────────────────────────────────────────────── */
    .skeleton-bubble {
      background-color: var(--muted, #f1f5f9);
      animation: skeleton-pulse 1.6s ease-in-out infinite;
      border-radius: var(--radius-xl, 20px);
    }
    @keyframes skeleton-pulse {
      0%, 100% { opacity: 1; }
      50%       { opacity: 0.45; }
    }
    .skeleton-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      flex-shrink: 0;
    }

    /* ── EPIC EMPTY STATE (Phase 2) ──────────────────────────── */
    .empty-state-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      padding: 2rem;
      animation: fadeIn 0.6s ease-out;
    }

    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to   { opacity: 1; transform: translateY(0); }
    }

    /* Glowing Orb Effect */
    .orb-container {
      position: relative;
      width: 88px;
      height: 88px;
      margin-bottom: 2rem;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .orb-pulse-1, .orb-pulse-2 {
      position: absolute;
      top: 0; left: 0; right: 0; bottom: 0;
      border-radius: 50%;
      background: var(--chat-primary, #0d9488);
      opacity: 0.15;
    }
    .orb-pulse-1 {
      animation: orbPulse 3s cubic-bezier(0.4, 0, 0.2, 1) infinite;
    }
    .orb-pulse-2 {
      animation: orbPulse 3s cubic-bezier(0.4, 0, 0.2, 1) infinite 1.5s;
    }

    @keyframes orbPulse {
      0% { transform: scale(1); opacity: 0.3; }
      100% { transform: scale(1.6); opacity: 0; }
    }

    .orb-core {
      position: relative;
      width: 72px;
      height: 72px;
      background: linear-gradient(135deg, var(--chat-secondary, #f0fdfa), #ffffff);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 2;
      box-shadow: 0 4px 20px rgba(13, 148, 136, 0.15), inset 0 2px 4px rgba(255,255,255,1);
      border: 1px solid var(--border-subtle, rgba(226, 232, 240, 0.8));
    }
    .orb-core i {
      font-size: 2.25rem;
      color: var(--chat-primary, #0d9488);
      filter: drop-shadow(0 2px 4px rgba(13, 148, 136, 0.2));
    }

    /* Typography */
    .empty-state-title {
      font-family: 'Inter', system-ui, sans-serif;
      font-weight: 600;
      font-size: 1.5rem;
      color: var(--text-main, #0f172a);
      letter-spacing: -0.02em;
      line-height: 1.4;
      margin-bottom: 0.75rem;
    }
    
    .empty-state-subtitle {
      font-size: 0.95rem;
      color: var(--text-muted, #64748b);
      max-width: 480px;
      line-height: 1.6;
      margin-bottom: 2.5rem;
    }

    /* Suggested Prompts Glass Grid */
    .prompts-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 1rem;
      margin-top: 2rem;
      width: 100%;
      max-width: 680px;
    }

    .prompt-card {
      background: var(--bg-glass, rgba(255, 255, 255, 0.75));
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border: 1px solid rgba(226, 232, 240, 0.8);
      border-radius: 12px;
      padding: 1.25rem;
      display: flex;
      align-items: center;
      gap: 1rem;
      text-align: left;
      cursor: pointer;
      color: var(--text-main, #0f172a);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.02);
      transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
      outline: none;
    }

    .prompt-icon {
      width: 36px;
      height: 36px;
      border-radius: 10px;
      background: var(--chat-secondary, #f0fdfa);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      color: var(--chat-primary, #0d9488);
      transition: all 0.3s ease;
    }

    .prompt-text {
      font-weight: 500;
      font-size: 0.9375rem;
      line-height: 1.4;
    }

    .prompt-card:hover {
      transform: translateY(-3px);
      border-color: var(--chat-primary, #0d9488);
      box-shadow: 0 10px 20px rgba(13, 148, 136, 0.06);
    }

    .prompt-card:hover .prompt-icon {
      background: var(--chat-primary, #0d9488);
      color: #ffffff;
      transform: scale(1.1) rotate(-5deg);
    }
  `],
  template: `
    <!-- ── Scrollable Messages Area ─────────────────────────── -->
    <div #scrollContainer class="scroll-area" (scroll)="onScroll()">
      <div #messageList class="message-list">

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
        @if (!isLoading && messages.length > 0) {
          @for (msg of messages; track msg.id) {
            <app-chat-message
              [message]="msg"
              (retryMessage)="retryMessage.emit($event)"
              (streamingStarted)="isAnyMessageStreaming.set(true)"
              (streamingFinished)="isAnyMessageStreaming.set(false)" />
          }

          <!-- Typing indicator -->
          <app-typing-indicator [visible]="isAiTyping && !isAnyMessageStreaming()" />
        }

        <!-- Physical spacer — replaces CSS bottom padding to give scrollHeight stable geometry -->
        <div class="bottom-spacer" style="height: 160px; flex-shrink: 0; pointer-events: none;"></div>
      </div>
    </div>

    <!-- ── EPIC EMPTY STATE (Phase 2) ─────────────────── -->
    @if (!isLoading && showSuggestedPrompts) {
      <div class="position-absolute top-0 start-0 w-100 h-100 pe-none">
        <div class="empty-state-container pe-auto">
          
          <!-- Glowing Orb Avatar -->
          <div class="orb-container">
            <div class="orb-pulse-1"></div>
            <div class="orb-pulse-2"></div>
            <div class="orb-core">
              <i class="bi bi-robot"></i>
            </div>
          </div>

          <!-- Elegant Typography -->
          <h2 class="empty-state-title">How can I help you today?</h2>
          <p class="empty-state-subtitle text-center">
            I'm here to listen and support you. You can share your feelings or choose a topic below to start a conversation.
          </p>
          
          <!-- Interactive Glass Grid -->
          <div class="prompts-grid">
            @for (prompt of suggestedPrompts; track prompt) {
              <button
                type="button"
                class="prompt-card"
                (click)="promptSelected.emit(prompt)">
                <div class="prompt-icon">
                  <i class="bi bi-chat-dots-fill"></i>
                </div>
                <span class="prompt-text">{{ prompt }}</span>
              </button>
            }
          </div>

        </div>
      </div>
    }
  `,
})
export class ChatWindow implements AfterViewInit, OnDestroy {
  @Input() messages: ChatMessageModel[]  = [];
  @Input() isAiTyping: boolean           = false;
  @Input() isLoading: boolean            = false;
  @Input() suggestedPrompts: string[]    = [];
  @Input() showSuggestedPrompts: boolean = false;

  @Output() promptSelected = new EventEmitter<string>();
  @Output() retryMessage   = new EventEmitter<ChatMessageModel>();

  @ViewChild('scrollContainer') private scrollContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('messageList')     private messageList!: ElementRef<HTMLDivElement>;

  readonly isAnyMessageStreaming = signal<boolean>(false);

  private readonly shouldAutoScroll = signal<boolean>(true);
  private resizeObserver: ResizeObserver | null = null;

  ngAfterViewInit(): void {
    this.resizeObserver = new ResizeObserver(() => {
      if (this.shouldAutoScroll()) {
        this.scrollToBottom();
      }
    });

    if (this.messageList?.nativeElement) {
      this.resizeObserver.observe(this.messageList.nativeElement);
    }
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
    this.resizeObserver = null;
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
    if (!this.scrollContainer?.nativeElement) return;
    const el = this.scrollContainer.nativeElement;
    el.scrollTop = el.scrollHeight;
  }
}
