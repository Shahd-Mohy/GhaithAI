import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { SessionSignalrService } from './session-signalr.service';

@Injectable()
export class WebrtcService {

  private peerConnection: RTCPeerConnection | null = null;
  private localStream: MediaStream | null = null;
  private remoteStream: MediaStream | null = null;
  private sessionId = '';

  private readonly ICE_SERVERS: RTCConfiguration = {
    iceServers: [{ urls: 'stun:stun.l.google.com:19302' }]
  };

  constructor(private signalr: SessionSignalrService) { }

  async getLocalStream(): Promise<MediaStream> {
    this.localStream = await navigator.mediaDevices.getUserMedia({ audio: true });
    return this.localStream;
  }

  getLocalStreamSync(): MediaStream | null {
    return this.localStream;
  }

  getRemoteStream(): MediaStream | null {
    return this.remoteStream;
  }

  // remoteTrackArrived$ is passed in from SessionOrchestratorService.
  // We emit it the moment the remote audio track lands so the patient
  // component transitions to InCall instantly — no 500 ms polling needed.
  createPeerConnection(
    sessionId: string,
    remoteTrackArrived$: Subject<MediaStream>
  ): RTCPeerConnection {
    this.sessionId = sessionId;
    this.peerConnection = new RTCPeerConnection(this.ICE_SERVERS);

    if (this.localStream) {
      this.localStream.getTracks().forEach(track => {
        this.peerConnection!.addTrack(track, this.localStream!);
      });
    }

    this.remoteStream = new MediaStream();

    this.peerConnection.ontrack = (event) => {
      event.streams[0].getTracks().forEach(track => {
        this.remoteStream!.addTrack(track);
      });
      // Emit immediately — subscriber attaches this stream to <audio>
      remoteTrackArrived$.next(this.remoteStream!);
    };

    this.peerConnection.onicecandidate = (event) => {
      if (event.candidate) {
        this.signalr.sendIceCandidate(this.sessionId, event.candidate.toJSON());
      }
    };

    return this.peerConnection;
  }

  async createOffer(): Promise<void> {
    if (!this.peerConnection) throw new Error('Peer connection not initialised.');

    if (this.peerConnection.localDescription?.type === 'offer') {
      await this.signalr.sendOffer(this.sessionId, this.peerConnection.localDescription.sdp);
      return;
    }

    if (this.peerConnection.signalingState !== 'stable') {
      return;
    }

    const offer = await this.peerConnection.createOffer();
    await this.peerConnection.setLocalDescription(offer);
    await this.signalr.sendOffer(this.sessionId, offer.sdp!);
  }

  async handleRemoteAnswer(sdp: string): Promise<void> {
    if (!this.peerConnection) throw new Error('Peer connection not initialised.');
    await this.peerConnection.setRemoteDescription({ type: 'answer', sdp });
  }

  async handleRemoteOfferAndAnswer(sdp: string): Promise<void> {
    if (!this.peerConnection) throw new Error('Peer connection not initialised.');
    await this.peerConnection.setRemoteDescription({ type: 'offer', sdp });
    const answer = await this.peerConnection.createAnswer();
    await this.peerConnection.setLocalDescription(answer);
    await this.signalr.sendAnswer(this.sessionId, answer.sdp!);
  }

  async addRemoteIceCandidate(candidate: RTCIceCandidateInit): Promise<void> {
    if (!this.peerConnection) return;
    await this.peerConnection.addIceCandidate(candidate);
  }

  close(): void {
    this.peerConnection?.close();
    this.peerConnection = null;
    this.localStream?.getTracks().forEach(t => t.stop());
    this.localStream = null;
    this.remoteStream = null;
  }
}
