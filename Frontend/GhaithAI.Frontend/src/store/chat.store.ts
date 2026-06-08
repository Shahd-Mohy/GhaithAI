// File: src/store/chat.store.ts

import { Injectable, signal, computed } from '@angular/core';
import { Subject } from 'rxjs';
import {
  ChatMessageModel,
  SessionModel,
  RiskAlertPayload,
  ConnectionStatus,
} from '../types/chat.types';

@Injectable({ providedIn: 'root' })
export class ChatStore {
  // ── Core Signals ────────────────────────────────────────────────────────────
  readonly activeSession    = signal<SessionModel | null>(null);
  readonly messages         = signal<ChatMessageModel[]>([]);
  readonly sessions         = signal<SessionModel[]>([]);
  readonly isAiTyping       = signal<boolean>(false);
  readonly typingBufferActive = signal<boolean>(false);
  readonly isRiskDetected   = signal<boolean>(false);
  readonly riskDetails      = signal<RiskAlertPayload | null>(null);
  readonly connectionStatus = signal<ConnectionStatus>('disconnected');
  readonly isLoadingHistory   = signal<boolean>(false);
  readonly isLoadingSessions  = signal<boolean>(false);
  readonly isSendingMessage   = signal<boolean>(false);
  readonly sessionsPage       = signal<number>(1);
  readonly sessionsTotalCount = signal<number>(0);
  readonly sessionsHasNext    = signal<boolean>(false);
  readonly isLoadingMoreSessions = signal<boolean>(false);

  // ── Computed Signals ─────────────────────────────────────────────────────────
  readonly hasActiveSession = computed(() => this.activeSession() !== null);
  readonly messageCount     = computed(() => this.messages().length);
  readonly isConnected      = computed(() => this.connectionStatus() === 'connected');
  readonly isReconnecting   = computed(() => this.connectionStatus() === 'reconnecting');
  readonly displayTyping    = computed(() => this.isAiTyping() || this.typingBufferActive());

  // ── Streams ──────────────────────────────────────────────────────────────────
  private readonly _apiResponseSubject = new Subject<ChatMessageModel>();
  readonly apiResponse$ = this._apiResponseSubject.asObservable();

  // ── Mutators ─────────────────────────────────────────────────────────────────

  /** Set or clear the active session. Clears messages when set to null. */
  setActiveSession(session: SessionModel | null): void {
    this.activeSession.set(session);
    if (session === null) {
      this.messages.set([]);
    }
  }

  /** Append a single message to the end of the list. */
  addMessage(msg: ChatMessageModel): void {
    if (msg.senderType === 'AI' && msg.status === 'sent') {
      this._apiResponseSubject.next(msg);
    } else {
      this.messages.update(msgs => [...msgs, msg]);
    }
  }

  /** Commit AI message after buffer completes */
  commitAiMessage(msg: ChatMessageModel): void {
    this.messages.update(msgs => [...msgs, msg]);
  }

  /** Remove a specific message from the list (e.g. before retrying an errored message). */
  removeMessage(id: string): void {
    this.messages.update(msgs => msgs.filter(m => m.id !== id));
  }

  /**
   * Replace an optimistic (pending) message with the confirmed server version.
   * Matches by the temporary client-generated id.
   */
  confirmOptimisticMessage(tempId: string, confirmed: ChatMessageModel): void {
    this.messages.update(msgs =>
      msgs.map(m => (m.id === tempId ? { ...confirmed, status: 'sent' as const } : m))
    );
  }

  /** Mark a message as errored (e.g. SignalR invoke failed). */
  markMessageError(tempId: string): void {
    this.messages.update(msgs =>
      msgs.map(m => (m.id === tempId ? { ...m, status: 'error' as const } : m))
    );
  }

  /** Prepend older messages to the front (used by infinite scroll / history load). */
  prependMessages(older: ChatMessageModel[]): void {
    this.messages.update(msgs => [...older, ...msgs]);
  }

  /** Replace the full sessions list. */
  setSessions(sessions: SessionModel[]): void {
    this.sessions.set(sessions);
  }

  appendSessions(newItems: SessionModel[]): void {
    this.sessions.update(existing => [...existing, ...newItems]);
  }

  /** Update a single session inside the sessions list (e.g. after session ends). */
  updateSessionInList(updated: SessionModel): void {
    this.sessions.update(list =>
      list.map(s => (s.id === updated.id ? updated : s))
    );
  }

  /** Remove a session from the list. Also clears the active session if it matches. */
  removeSession(sessionId: string): void {
    this.sessions.update(list => list.filter(s => s.id !== sessionId));
    if (this.activeSession()?.id === sessionId) {
      this.setActiveSession(null);
    }
  }

  /** Patch only the title of a session in the list. */
  updateSessionTitle(sessionId: string, newTitle: string): void {
    this.sessions.update(list =>
      list.map(s => s.id === sessionId ? { ...s, title: newTitle } : s)
    );
    // Also patch the active session if it is the same one
    if (this.activeSession()?.id === sessionId) {
      const current = this.activeSession();
      if (current) this.activeSession.set({ ...current, title: newTitle });
    }
  }

  /** Store a risk alert and flag risk as detected. */
  setRiskAlert(payload: RiskAlertPayload): void {
    this.isRiskDetected.set(true);
    this.riskDetails.set(payload);
  }

  /** Clear the risk alert (e.g. user dismissed the crisis overlay). */
  dismissRiskAlert(): void {
    this.isRiskDetected.set(false);
    this.riskDetails.set(null);
  }

  /** Set sending message state. */
  setSendingMessage(val: boolean): void {
    this.isSendingMessage.set(val);
  }

  /** Full reset — called on logout or session cleanup. */
  reset(): void {
    this.activeSession.set(null);
    this.messages.set([]);
    this.sessions.set([]);
    this.isAiTyping.set(false);
    this.typingBufferActive.set(false);
    this.isRiskDetected.set(false);
    this.riskDetails.set(null);
    this.connectionStatus.set('disconnected');
    this.isLoadingHistory.set(false);
    this.isLoadingSessions.set(false);
    this.isSendingMessage.set(false);
    this.sessionsPage.set(1);
    this.sessionsTotalCount.set(0);
    this.sessionsHasNext.set(false);
    this.isLoadingMoreSessions.set(false);
  }
}
