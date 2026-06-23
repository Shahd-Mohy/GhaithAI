import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';

export interface RtcOfferMessage {
  sessionId: string;
  sdp: string;
}

export interface RtcAnswerMessage {
  sessionId: string;
  sdp: string;
}

export interface RtcIceCandidateMessage {
  sessionId: string;
  candidate: RTCIceCandidateInit;
}

@Injectable({ providedIn: 'root' })
export class SessionSignalrService {

  private hubConnection: signalR.HubConnection | null = null;

  readonly offerReceived$ = new Subject<RtcOfferMessage>();
  readonly answerReceived$ = new Subject<RtcAnswerMessage>();
  readonly iceCandidateReceived$ = new Subject<RtcIceCandidateMessage>();

  async connect(sessionId: string, accessToken: string): Promise<void> {
    // Use the full backend base URL — relative paths go to Angular dev server (4200), not backend (53898)
    const hubUrl = `${environment.apiUrl.replace('/api', '')}/hubs/session?sessionId=${sessionId}`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessToken
      })
      .withAutomaticReconnect()
      .build();

    this.registerHandlers();

    await this.hubConnection.start();
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveOffer', (msg: RtcOfferMessage) => {
      this.offerReceived$.next(msg);
    });

    this.hubConnection.on('ReceiveAnswer', (msg: RtcAnswerMessage) => {
      this.answerReceived$.next(msg);
    });

    this.hubConnection.on('ReceiveIceCandidate', (msg: RtcIceCandidateMessage) => {
      this.iceCandidateReceived$.next(msg);
    });
  }

  async sendOffer(sessionId: string, sdp: string): Promise<void> {
    await this.hubConnection?.invoke('SendOffer', sessionId, sdp);
  }

  async sendAnswer(sessionId: string, sdp: string): Promise<void> {
    await this.hubConnection?.invoke('SendAnswer', sessionId, sdp);
  }

  async sendIceCandidate(sessionId: string, candidate: RTCIceCandidateInit): Promise<void> {
    await this.hubConnection?.invoke('SendIceCandidate', sessionId, candidate);
  }

  async disconnect(): Promise<void> {
    await this.hubConnection?.stop();
    this.hubConnection = null;
  }
}