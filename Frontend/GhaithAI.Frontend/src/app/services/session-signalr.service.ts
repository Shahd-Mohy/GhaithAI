import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

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

  // الفرونت إند بيستمع على الـ Subjects دي — مش بيتعامل مع الـ hub مباشرة
  readonly offerReceived$ = new Subject<RtcOfferMessage>();
  readonly answerReceived$ = new Subject<RtcAnswerMessage>();
  readonly iceCandidateReceived$ = new Subject<RtcIceCandidateMessage>();

  async connect(sessionId: string, accessToken: string): Promise<void> {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`/hubs/session?sessionId=${sessionId}`, {
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
