// File: src/app/support/chat/page.ts

import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  inject,
} from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, switchMap, filter } from 'rxjs/operators';

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
    :host {
      display: flex;
      flex-direction: column;
      flex: 1 1 0%;
      height: 100%;
      overflow: hidden;
      background-color: var(--chat-bg, #ffffff);
    }
    .chat-page-inner {
      flex: 1 1 0%;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      max-width: 900px;
      margin: 0 auto;
      width: 100%;
      padding: 1rem;
    }
    @media (min-width: 768px) {
      .chat-page-inner { padding: 1.5rem; }
    }
    @media (min-width: 992px) {
      .chat-page-inner { padding: 2rem; }
    }
    .status-banner {
      flex-shrink: 0;
      text-align: center;
      font-size: 0.8125rem;
      padding: 0.4rem 1rem;
      font-weight: 500;
      background-color: var(--chat-secondary, #f5f5f5);
      color: var(--chat-secondary-fg, #1a1a1a);
    }
    .input-wrapper {
      flex-shrink: 0;
      padding-top: 1rem;
    }
  `],
  template: `
    <!-- Connection Status Banner -->
    @if (chatStore.connectionStatus() === 'connecting' || chatStore.connectionStatus() === 'reconnecting') {
      <div class="status-banner">
        <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
        Connecting to GhaithAI...
      </div>
    }

    <!-- Crisis Overlay -->
    <app-crisis-overlay
      [visible]="chatStore.isRiskDetected()"
      [suggestedAction]="chatStore.riskDetails()?.suggestedAction || ''"
      (getCrisisHelp)="onCrisisHelpRequested()"
      (dismissed)="onCrisisDismissed()" />

    <div class="chat-page-inner">
      <!-- Chat Window Area -->
      <app-chat-window
        class="d-flex flex-column flex-grow-1 overflow-hidden"
        [messages]="chatStore.messages()"
        [isAiTyping]="chatStore.isAiTyping()"
        [isLoading]="chatStore.isLoadingHistory()"
        [suggestedPrompts]="suggestedPrompts"
        [showSuggestedPrompts]="chatStore.messages().length === 0"
        (promptSelected)="onMessageSent($event)"
        (retryMessage)="onRetryMessage($event)"
      />

      <!-- Input Area -->
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

      // If no active session, create one first!
      if (!session) {
        session = await this.createNewSession();
        if (!session) {
          this.chatStore.isSendingMessage.set(false);
          return;
        }
      }

      // Add optimistic message to UI
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
      this.chatStore.isAiTyping.set(true);

      // Send via SignalR
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
        this.chatStore.markMessageError(tempId);
        this.chatStore.isAiTyping.set(false);
      }
    } finally {
      this.chatStore.isSendingMessage.set(false);
    }
  }

  onRetryMessage(msg: ChatMessageModel): void {
    // Remove the failed message and try sending its content again
    this.chatStore.removeMessage(msg.id);
    this.onMessageSent(msg.content);
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
          
          // Also prepend the new session to the sidebar list
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
