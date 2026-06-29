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

export interface PatientReadyMessage {
  sessionId: string;
}

export interface DoctorJoinedMessage {
  sessionId: string;
}

@Injectable()
export class SessionSignalrService {

  private hubConnection: signalR.HubConnection | null = null;

  readonly offerReceived$ = new Subject<RtcOfferMessage>();
  readonly answerReceived$ = new Subject<RtcAnswerMessage>();
  readonly iceCandidateReceived$ = new Subject<RtcIceCandidateMessage>();
  readonly patientReady$ = new Subject<PatientReadyMessage>();
  readonly doctorJoined$ = new Subject<DoctorJoinedMessage>();

  async connect(sessionId: string, accessToken: string): Promise<void> {
    const cleanSessionId = sessionId.trim().toLowerCase();
    const hubUrl = `${environment.apiUrl.replace('/api', '')}/hubs/session?sessionId=${cleanSessionId}`;

    console.log(`[SignalR] Connecting to ${hubUrl}...`);
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessToken
      })
      .withAutomaticReconnect()
      .build();

    this.registerHandlers();
    await this.hubConnection.start();
    console.log(`[SignalR] Connected successfully. Invoking JoinSession with: ${cleanSessionId}`);
    await this.hubConnection.invoke('JoinSession', cleanSessionId);
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveOffer', (msg: RtcOfferMessage) => {
      console.log('[SignalR] ReceiveOffer received:', msg);
      this.offerReceived$.next(msg);
    });

    this.hubConnection.on('ReceiveAnswer', (msg: RtcAnswerMessage) => {
      console.log('[SignalR] ReceiveAnswer received:', msg);
      this.answerReceived$.next(msg);
    });

    this.hubConnection.on('ReceiveIceCandidate', (msg: RtcIceCandidateMessage) => {
      console.log('[SignalR] ReceiveIceCandidate received:', msg);
      this.iceCandidateReceived$.next(msg);
    });

    this.hubConnection.on('PatientReady', (msg: PatientReadyMessage) => {
      console.log('[SignalR] PatientReady received:', msg);
      this.patientReady$.next(msg);
    });

    this.hubConnection.on('DoctorJoined', (msg: DoctorJoinedMessage) => {
      console.log('[SignalR] DoctorJoined received:', msg);
      this.doctorJoined$.next(msg);
    });
  }

  async notifyPatientReady(sessionId: string): Promise<void> {
    const cleanId = sessionId.trim().toLowerCase();
    console.log('[SignalR] notifyPatientReady:', cleanId);
    await this.hubConnection?.invoke('PatientReady', cleanId);
  }

  async notifyDoctorJoined(sessionId: string): Promise<void> {
    const cleanId = sessionId.trim().toLowerCase();
    console.log('[SignalR] notifyDoctorJoined:', cleanId);
    await this.hubConnection?.invoke('DoctorJoined', cleanId);
  }

  async sendOffer(sessionId: string, sdp: string): Promise<void> {
    const cleanId = sessionId.trim().toLowerCase();
    console.log('[SignalR] sendOffer:', cleanId);
    await this.hubConnection?.invoke('SendOffer', cleanId, sdp);
  }

  async sendAnswer(sessionId: string, sdp: string): Promise<void> {
    const cleanId = sessionId.trim().toLowerCase();
    console.log('[SignalR] sendAnswer:', cleanId);
    await this.hubConnection?.invoke('SendAnswer', cleanId, sdp);
  }

  async sendIceCandidate(sessionId: string, candidate: RTCIceCandidateInit): Promise<void> {
    const cleanId = sessionId.trim().toLowerCase();
    console.log('[SignalR] sendIceCandidate:', cleanId);
    await this.hubConnection?.invoke('SendIceCandidate', cleanId, candidate);
  }

  async disconnect(): Promise<void> {
    console.log('[SignalR] Disconnecting...');
    await this.hubConnection?.stop();
    this.hubConnection = null;
  }
}
