// File: src/hooks/useSignalR.ts

import { Injectable, inject, NgZone, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
import { ChatStore } from '../store/chat.store';
import { CrisisStore } from '../store/crisis.store';
import {
  ChatMessageModel,
  RiskAlertPayload,
  SessionModel,
  UserChatRequest,
  StartSessionRequest,
} from '../types/chat.types';
import { environment } from '../environments/environment';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hub!: signalR.HubConnection;

  private readonly chatStore   = inject(ChatStore);
  private readonly crisisStore = inject(CrisisStore);
  private readonly ngZone      = inject(NgZone);
  private readonly platformId  = inject(PLATFORM_ID);

  // ── Observable Streams (for components that need to subscribe directly) ─────
  private readonly messageSubject        = new Subject<ChatMessageModel>();
  private readonly sessionStartedSubject = new Subject<SessionModel>();
  private readonly sessionEndedSubject   = new Subject<SessionModel>();

  readonly message$        : Observable<ChatMessageModel> = this.messageSubject.asObservable();
  readonly sessionStarted$ : Observable<SessionModel>     = this.sessionStartedSubject.asObservable();
  readonly sessionEnded$   : Observable<SessionModel>     = this.sessionEndedSubject.asObservable();

  // ── Connection Setup ─────────────────────────────────────────────────────────

  buildConnection(): void {
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalRUrl, {
        // JWT is read from query string on the backend:
        // OnMessageReceived reads context.Request.Query["access_token"]
        accessTokenFactory: () =>
          isPlatformBrowser(this.platformId)
            ? (localStorage.getItem(environment.jwtKey) ?? '')
            : '',
        // transport: signalR.HttpTransportType.WebSockets, // Commented out to allow negotiation fallback
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.registerServerEvents();
    this.registerLifecycleHooks();
  }

  startConnection(): Promise<void> {
    // Never attempt a WebSocket connection during SSR
    if (!isPlatformBrowser(this.platformId)) {
      return Promise.resolve();
    }

    if (!this.hub) {
      this.buildConnection();
    }

    this.chatStore.connectionStatus.set('connecting');
    return this.hub.start().then(() => {
      this.ngZone.run(() => this.chatStore.connectionStatus.set('connected'));
    });
  }

  stopConnection(): Promise<void> {
    return this.hub?.stop() ?? Promise.resolve();
  }

  // ── Client → Server Invocations ──────────────────────────────────────────────

  /** Invoke 'SendMessage' on the hub — primary way to send user messages. */
  sendMessage(dto: UserChatRequest): Promise<void> {
    return this.hub.invoke('SendMessage', dto);
  }

  /** Invoke 'StartSession' on the hub — creates a new chat session via SignalR. */
  startSession(dto: StartSessionRequest): Promise<void> {
    return this.hub.invoke('StartSession', dto);
  }

  /** Invoke 'EndSession' on the hub — ends the active session. */
  endSession(sessionId: string): Promise<void> {
    return this.hub.invoke('EndSession', sessionId);
  }

  // ── Server → Client Events ───────────────────────────────────────────────────

  private registerServerEvents(): void {
    // 1. AI typing indicator — show/hide animated dots
    this.hub.on('AiTyping', (isTyping: boolean) => {
      try {
        this.ngZone.run(() => this.chatStore.isAiTyping.set(isTyping));
      } catch (err) {
        console.error('[SignalR] AiTyping handler error:', err);
      }
    });

    // 2. AI message received — add to message list
    this.hub.on('ReceiveMessage', (msg: ChatMessageModel) => {
      try {
        this.ngZone.run(() => {
          const mapped: ChatMessageModel = { ...msg, status: 'sent' };
          this.chatStore.addMessage(mapped);
          this.messageSubject.next(mapped);
        });
      } catch (err) {
        console.error('[SignalR] ReceiveMessage handler error:', err);
      }
    });

    // 3. Risk alert — trigger crisis overlay
    this.hub.on('RiskAlert', (payload: RiskAlertPayload) => {
      try {
        this.ngZone.run(() => {
          this.chatStore.setRiskAlert(payload);
          this.crisisStore.trigger(payload.suggestedAction);
        });
      } catch (err) {
        console.error('[SignalR] RiskAlert handler error:', err);
      }
    });

    // 4. Session started (via SignalR StartSession invoke)
    this.hub.on('SessionStarted', (session: SessionModel) => {
      try {
        this.ngZone.run(() => {
          this.chatStore.setActiveSession(session);
          this.sessionStartedSubject.next(session);
        });
      } catch (err) {
        console.error('[SignalR] SessionStarted handler error:', err);
      }
    });

    // 5. Session ended (via SignalR EndSession invoke)
    this.hub.on('SessionEnded', (session: SessionModel) => {
      try {
        this.ngZone.run(() => {
          this.chatStore.updateSessionInList(session);
          this.sessionEndedSubject.next(session);
        });
      } catch (err) {
        console.error('[SignalR] SessionEnded handler error:', err);
      }
    });
  }

  // ── Connection Lifecycle Hooks ────────────────────────────────────────────────

  private registerLifecycleHooks(): void {
    this.hub.onreconnecting(() => {
      this.ngZone.run(() => this.chatStore.connectionStatus.set('reconnecting'));
    });

    this.hub.onreconnected(() => {
      this.ngZone.run(() => this.chatStore.connectionStatus.set('connected'));
    });

    this.hub.onclose(() => {
      this.ngZone.run(() => this.chatStore.connectionStatus.set('disconnected'));
    });
  }
}
