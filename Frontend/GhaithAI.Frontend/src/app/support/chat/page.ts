// File: src/app/support/chat/page.ts

import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  inject,
} from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, zip, timer } from 'rxjs';
import { takeUntil, take, filter } from 'rxjs/operators';

import { ChatStore } from '../../../store/chat.store';
import { SignalRService } from '../../../hooks/useSignalR';
import { ChatHttpService } from '../../../services/chat.service';
import { ChatMessageModel, SessionModel } from '../../../types/chat.types';

import { ChatWindow } from '../../../components/chat/ChatWindow';
import { ChatInput } from '../../../components/chat/ChatInput';
import { CrisisOverlay } from '../../../components/chat/CrisisOverlay';

@Component({
  selector: 'app-chat-page',
  standalone: true,
  imports: [ChatWindow, ChatInput, CrisisOverlay],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    /* ══════════════════════════════════════════════════════════
       HOST SHELL — Full viewport flex column
    ══════════════════════════════════════════════════════════ */
    :host {
      display: flex;
      flex-direction: column;
      flex: 1 1 0%;
      height: 100vh;
      overflow: hidden;
      background-color: var(--workspace-bg, #ffffff);
      font-family: 'Inter', system-ui, sans-serif;
      position: relative;
    }

    /* ══════════════════════════════════════════════════════════
       GLASSMORPHIC STICKY HEADER
    ══════════════════════════════════════════════════════════ */
    .chat-header {
      flex-shrink: 0;
      position: sticky;
      top: 0;
      z-index: 50;
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem 1.5rem;
      background: var(--bg-glass, rgba(255, 255, 255, 0.75));
      backdrop-filter: blur(12px) saturate(180%);
      -webkit-backdrop-filter: blur(12px) saturate(180%);
      border-bottom: 1px solid var(--border-subtle, rgba(226, 232, 240, 0.8));
      box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.04);
    }

    /* Brand cluster */
    .brand-cluster {
      display: flex;
      align-items: center;
      gap: 0.625rem;
    }

    .brand-icon-wrap {
      width: 34px;
      height: 34px;
      border-radius: 9px;
      background: linear-gradient(135deg, var(--chat-primary, #0d9488) 0%, var(--chat-primary-dark, #0f766e) 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 2px 8px rgba(13, 148, 136, 0.3);
      flex-shrink: 0;
    }

    .brand-icon-wrap i {
      font-size: 1rem;
      color: #ffffff;
    }

    .brand-wordmark {
      font-family: 'Inter', system-ui, sans-serif;
      font-weight: 700;
      font-size: 1.0625rem;
      letter-spacing: -0.03em;
      color: var(--text-main, #0f172a);
      margin: 0;
      line-height: 1;
    }

    .brand-wordmark span {
      color: var(--chat-primary, #0d9488);
    }

    /* ══════════════════════════════════════════════════════════
       CONNECTION STATUS BADGE (inline in header cluster)
    ══════════════════════════════════════════════════════════ */
    .conn-badge {
      display: inline-flex;
      align-items: center;
      gap: 5px;
      font-size: 0.6875rem;
      font-weight: 500;
      letter-spacing: 0.01em;
      padding: 3px 8px;
      border-radius: 999px;
      background: rgba(13, 148, 136, 0.08);
      color: var(--chat-primary, #0d9488);
      border: 1px solid rgba(13, 148, 136, 0.18);
    }

    .conn-badge.is-reconnecting {
      background: rgba(234, 179, 8, 0.1);
      color: #a16207;
      border-color: rgba(234, 179, 8, 0.25);
    }

    .conn-dot {
      width: 6px;
      height: 6px;
      border-radius: 50%;
      background: currentColor;
    }

    .conn-dot.is-live {
      animation: teal-breathe 2s ease-in-out infinite;
    }

    @keyframes teal-breathe {
      0%, 100% { box-shadow: 0 0 0 0 rgba(13, 148, 136, 0.3); }
      50%       { box-shadow: 0 0 0 5px rgba(13, 148, 136, 0); }
    }

    /* ══════════════════════════════════════════════════════════
       CRISIS SUPPORT BUTTON — Polished Accent
    ══════════════════════════════════════════════════════════ */
    .btn-crisis {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 6px 14px;
      font-family: 'Inter', system-ui, sans-serif;
      font-size: 0.8125rem;
      font-weight: 600;
      letter-spacing: -0.005em;
      color: var(--destructive, #dc2626);
      background: rgba(220, 38, 38, 0.05);
      border: 1.5px solid rgba(220, 38, 38, 0.25);
      border-radius: 999px;
      cursor: pointer;
      transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
      white-space: nowrap;
      text-decoration: none;
    }

    .btn-crisis:hover {
      background: rgba(220, 38, 38, 0.1);
      border-color: rgba(220, 38, 38, 0.45);
      color: var(--destructive, #dc2626);
      transform: translateY(-1px);
      box-shadow: 0 4px 14px rgba(220, 38, 38, 0.18);
      animation: pulse-ring 1.8s ease-in-out infinite;
    }

    .btn-crisis:active {
      transform: translateY(0px);
      box-shadow: none;
    }

    @keyframes pulse-ring {
      0%, 100% { box-shadow: 0 0 0 0 rgba(220, 38, 38, 0.2); }
      50%       { box-shadow: 0 0 0 6px rgba(220, 38, 38, 0); }
    }

    .btn-crisis .crisis-dot {
      width: 7px;
      height: 7px;
      border-radius: 50%;
      background: var(--destructive, #dc2626);
      flex-shrink: 0;
    }

    /* ══════════════════════════════════════════════════════════
       STATUS BANNER — Reconnection notice
    ══════════════════════════════════════════════════════════ */
    .status-banner {
      flex-shrink: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      text-align: center;
      font-family: 'Inter', system-ui, sans-serif;
      font-size: 0.8125rem;
      font-weight: 500;
      padding: 0.5rem 1rem;
      background: linear-gradient(90deg,
        rgba(13, 148, 136, 0.04) 0%,
        rgba(13, 148, 136, 0.08) 50%,
        rgba(13, 148, 136, 0.04) 100%);
      color: var(--chat-primary, #0d9488);
      border-bottom: 1px solid rgba(13, 148, 136, 0.12);
      animation: slideDown 0.3s cubic-bezier(0.4, 0, 0.2, 1) forwards;
    }

    @keyframes slideDown {
      from { opacity: 0; transform: translateY(-6px); }
      to   { opacity: 1; transform: translateY(0); }
    }

    .banner-spinner {
      width: 14px;
      height: 14px;
      border: 2px solid rgba(13, 148, 136, 0.2);
      border-top-color: var(--chat-primary, #0d9488);
      border-radius: 50%;
      animation: spin 0.7s linear infinite;
      flex-shrink: 0;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    /* ══════════════════════════════════════════════════════════
       CHAT INNER CONTAINER — Constrained centered column
    ══════════════════════════════════════════════════════════ */
    .chat-page-inner {
      flex: 1 1 0%;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      max-width: 800px;
      margin: 0 auto;
      width: 100%;
      padding: 0;
    }

    /* ══════════════════════════════════════════════════════════
       INPUT WRAPPER ANCHOR
    ══════════════════════════════════════════════════════════ */
    .input-wrapper {
      flex-shrink: 0;
    }
  `],
  template: `
    <!-- ══════════════════════════════════════════════════════
         GLASSMORPHIC STICKY HEADER
    ══════════════════════════════════════════════════════ -->
    <header class="chat-header">

      <!-- Brand Cluster -->
      <div class="brand-cluster">
        <div class="brand-icon-wrap">
          <i class="bi bi-robot"></i>
        </div>
        <h1 class="brand-wordmark">Ghaith<span>AI</span></h1>

        <!-- Live connection status badge -->
        @if (chatStore.connectionStatus() === 'connected') {
          <span class="conn-badge">
            <span class="conn-dot is-live"></span>
            Live
          </span>
        }
        @if (chatStore.connectionStatus() === 'reconnecting') {
          <span class="conn-badge is-reconnecting">
            <span class="conn-dot"></span>
            Reconnecting
          </span>
        }
      </div>

      <!-- Crisis Support CTA -->
      <button
        type="button"
        class="btn-crisis"
        (click)="onCrisisHelpRequested()"
        aria-label="Access crisis support resources">
        <span class="crisis-dot" aria-hidden="true"></span>
        <i class="bi bi-telephone-fill" style="font-size: 0.75rem;"></i>
        <span class="d-none d-sm-inline">Crisis Support</span>
      </button>

    </header>

    <!-- ══════════════════════════════════════════════════════
         CONNECTION STATUS BANNER (connecting / reconnecting)
    ══════════════════════════════════════════════════════ -->
    @if (chatStore.connectionStatus() === 'connecting' || chatStore.connectionStatus() === 'reconnecting') {
      <div class="status-banner" role="status" aria-live="polite">
        <span class="banner-spinner" aria-hidden="true"></span>
        @if (chatStore.connectionStatus() === 'connecting') {
          Establishing secure connection…
        } @else {
          Connection lost — reconnecting automatically…
        }
      </div>
    }

    <!-- ══════════════════════════════════════════════════════
         CRISIS OVERLAY (auto AI risk detection)
    ══════════════════════════════════════════════════════ -->
    <app-crisis-overlay
      [visible]="chatStore.isRiskDetected()"
      [suggestedAction]="chatStore.riskDetails()?.suggestedAction || ''"
      (getCrisisHelp)="onCrisisHelpRequested()"
      (dismissed)="onCrisisDismissed()" />

    <!-- ══════════════════════════════════════════════════════
         MAIN CHAT BODY — Window + Input
    ══════════════════════════════════════════════════════ -->
    <div class="chat-page-inner">

      <!-- Chat Window — Scrollable Messages Area -->
      <app-chat-window
        class="d-flex flex-column flex-grow-1 overflow-hidden"
        [messages]="chatStore.messages()"
        [isAiTyping]="chatStore.displayTyping()"
        [isLoading]="chatStore.isLoadingHistory()"
        [suggestedPrompts]="suggestedPrompts"
        [showSuggestedPrompts]="chatStore.messages().length === 0"
        (promptSelected)="onMessageSent($event)"
        (retryMessage)="onRetryMessage($event)"
      />

      <!-- Input Footer — Anchored to bottom -->
      <div class="input-wrapper">
        <app-chat-input
          [disabled]="chatStore.isSendingMessage() || chatStore.connectionStatus() !== 'connected'"
          (messageSent)="onMessageSent($event)"
        />
      </div>

    </div>
  `
})
export class ChatPage implements OnInit, OnDestroy {
  readonly chatStore = inject(ChatStore);
  readonly signalRService = inject(SignalRService);
  private readonly chatHttpService = inject(ChatHttpService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();

  readonly suggestedPrompts = [
    "I've been feeling anxious lately",
    "I had a difficult day",
    "I need someone to talk to",
    "Help me understand my feelings"
  ];

  ngOnInit(): void {
    // 1. Start SignalR Connection
    this.chatStore.connectionStatus.set('connecting');
    this.signalRService.startConnection().catch(err => {
      console.error('[ChatPage] SignalR Connection Error:', err);
    });

    // 2. Listen to route query params for sessionId
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const sessionId = params['sessionId'];
        if (sessionId) {
          this.loadSession(sessionId);
        } else {
          // No sessionId -> New conversation state
          this.chatStore.setActiveSession(null);
          this.chatStore.isAiTyping.set(false);
          this.chatStore.isLoadingHistory.set(false);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalRService.stopConnection();
  }

  // ── Session Loading ──────────────────────────────────────────────────────────

  private loadSession(sessionId: string): void {
    // If it's already the active session, do nothing
    if (this.chatStore.activeSession()?.id === sessionId) return;

    this.chatStore.isLoadingHistory.set(true);
    this.chatStore.setActiveSession(null); // Clear old messages immediately

    this.chatHttpService.getSessionHistory(sessionId).subscribe({
      next: (res) => {
        this.chatStore.setActiveSession(res.session);
        this.chatStore.messages.set(res.messages);
        this.chatStore.isLoadingHistory.set(false);
      },
      error: (err) => {
        console.error('[ChatPage] Failed to load session history:', err);
        this.chatStore.isLoadingHistory.set(false);
        // Fallback: navigate to new conversation
        this.router.navigate(['/support/chat']);
      }
    });
  }

  // ── Message Handling ─────────────────────────────────────────────────────────

  async onMessageSent(content: string): Promise<void> {
    if (!content || !content.trim()) return;

    this.chatStore.isSendingMessage.set(true);

    try {
      let session = this.chatStore.activeSession();

      // If no active session, create one first
      if (!session) {
        session = await this.createNewSession();
        if (!session) {
          this.chatStore.isSendingMessage.set(false);
          return;
        }
      }

      // Add optimistic message to UI immediately
      const tempId = crypto.randomUUID();
      const optimisticMsg: ChatMessageModel = {
        id: tempId,
        senderType: 'User',
        content: content.trim(),
        sentAt: new Date().toISOString(),
        isOptimistic: true,
        status: 'sending'
      };
      this.chatStore.addMessage(optimisticMsg);
      this.chatStore.typingBufferActive.set(true);

      const responseSubject = new Subject<ChatMessageModel>();
      
      const responseSubscription = zip(
        responseSubject.pipe(take(1)),
        timer(1200)
      ).subscribe({
        next: ([aiMsg]) => {
          this.chatStore.typingBufferActive.set(false);
          this.chatStore.commitAiMessage(aiMsg);
        }
      });

      const apiSub = this.chatStore.apiResponse$.pipe(take(1)).subscribe(msg => {
        responseSubject.next(msg);
      });

      // Send via SignalR (primary channel)
      try {
        await this.signalRService.sendMessage({
          sessionId: session.id,
          message: content.trim()
        });

        // Mark optimistic message as confirmed
        this.chatStore.confirmOptimisticMessage(tempId, {
          ...optimisticMsg,
          isOptimistic: false,
          status: 'sent'
        });
      } catch (err) {
        console.error('[ChatPage] SendMessage failed:', err);
        apiSub.unsubscribe();
        this.chatStore.markMessageError(tempId);

        // Append Empathetic Fallback Alert via the response pipeline
        const fallbackId = crypto.randomUUID();
        responseSubject.next({
          id: fallbackId,
          senderType: 'AI',
          content: "I'm having a little trouble connecting right now. Don't worry, your thoughts are safe and won't be lost. Whenever you're ready, let's try again.",
          sentAt: new Date().toISOString(),
          isOptimistic: false,
          status: 'error'
        });
      }
    } finally {
      this.chatStore.isSendingMessage.set(false);
    }
  }

  onRetryMessage(msg: ChatMessageModel): void {
    if (msg.senderType === 'AI' && msg.status === 'error') {
      this.chatStore.removeMessage(msg.id);
      const messages = this.chatStore.messages();
      const lastUserMsg = [...messages].reverse().find(m => m.senderType === 'User' && m.status === 'error');
      if (lastUserMsg) {
        this.chatStore.removeMessage(lastUserMsg.id);
        this.onMessageSent(lastUserMsg.content);
      }
    } else {
      this.chatStore.removeMessage(msg.id);
      this.onMessageSent(msg.content);
    }
  }

  private createNewSession(): Promise<SessionModel | null> {
    return new Promise((resolve) => {
      this.chatHttpService.startSession({ memoryEnabled: true }).subscribe({
        next: (session) => {
          this.chatStore.setActiveSession(session);

          // Update the URL with the new session ID so the user can refresh
          // Use replaceUrl: true so clicking "back" takes them out of the chat entirely
          this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { sessionId: session.id },
            queryParamsHandling: 'merge',
            replaceUrl: true
          });

          // Prepend the new session to the sidebar history list
          const currentSessions = this.chatStore.sessions();
          this.chatStore.setSessions([session, ...currentSessions]);

          resolve(session);
        },
        error: (err) => {
          console.error('[ChatPage] Failed to start new session:', err);
          resolve(null);
        }
      });
    });
  }

  // ── Crisis Handlers ──────────────────────────────────────────────────────────

  onCrisisHelpRequested(): void {
    this.router.navigate(['/support/crisis']);
  }

  onCrisisDismissed(): void {
    this.chatStore.dismissRiskAlert();
  }
}
