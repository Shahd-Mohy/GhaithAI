// File: src/types/chat.types.ts

// ============================================================
// OUTGOING: What Angular sends to the Backend
// ============================================================

/** Mirrors: StartSessionDTO */
export interface StartSessionRequest {
  memoryEnabled: boolean;
  title?: string;
}

/** Mirrors: UserChatRequestDto */
export interface UserChatRequest {
  sessionId: string; // GUID as string
  message: string;
}

// ============================================================
// INCOMING: What Angular receives from the Backend
// ============================================================

/** Mirrors: SessionDTO */
export interface SessionModel {
  id: string;                          // GUID
  status: 'active' | 'ended';
  riskLevel: 'low' | 'medium' | 'high';
  memoryEnabled: boolean;
  title?: string;
  emotionalTone?: string;
  aiSummary?: string;
  startedAt: string;                   // ISO date string
  endedAt?: string;                    // ISO date string
  messageCount: number;
}

/** Mirrors: ChatMessageDTO (+ frontend-only fields) */
export interface ChatMessageModel {
  id: string;                          // GUID
  senderType: 'User' | 'AI';
  content: string;
  sentimentScore?: number;
  detectedEmotion?: string;
  detectedLanguage?: string;
  sentAt: string;                      // ISO date string
  // Frontend-only fields — never sent to backend
  isOptimistic?: boolean;              // true = pending server confirmation
  status?: 'sending' | 'sent' | 'error';
}

/** Mirrors: RiskDetailsDto */
export interface RiskDetails {
  riskType: string;
  detectedMarkers: string;
  confidenceScore: number;
  supportingContext: string;
  suggestedAction: string;
}

/** Mirrors: SignalR RiskAlert event payload */
export interface RiskAlertPayload {
  riskType: string;
  suggestedAction: string;
  confidenceScore: number;
  isRiskDetected: boolean;
}

/** Mirrors: SendMessageResponseDTO */
export interface SendMessageResponse {
  userMessage: ChatMessageModel;
  aiMessage: ChatMessageModel;
  isRiskDetected: boolean;
  riskDetails?: RiskDetails;
}

/** Mirrors: ChatHistoryDTO */
export interface ChatHistoryResponse {
  session: SessionModel;
  messages: ChatMessageModel[];
}

/** Mirrors: PaginatedSessionsDTO */
export interface PaginatedSessions {
  items: SessionModel[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// ============================================================
// FRONTEND-ONLY: UI State Types
// ============================================================

export type ConnectionStatus = 'connected' | 'connecting' | 'reconnecting' | 'disconnected';

/** Full UI state shape — represented as individual signals in ChatStore */
export interface ChatUiState {
  activeSession: SessionModel | null;
  messages: ChatMessageModel[];
  sessions: SessionModel[];
  isAiTyping: boolean;
  isRiskDetected: boolean;
  riskDetails: RiskAlertPayload | null;
  connectionStatus: ConnectionStatus;
  isLoadingHistory: boolean;
  isLoadingSessions: boolean;
  isSendingMessage: boolean;
}
