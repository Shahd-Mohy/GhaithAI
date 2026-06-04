using GhaithAI.API.DTOs.Chat;
using GhaithAI.API.GaithAI.Application.DTOs.Chat;

namespace GhaithAI.API.Services.Interfaces
{
    /// <summary>
    /// Application service for managing chat sessions and message flow.
    /// Orchestrates session lifecycle and delegates AI processing to LangflowService.
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Starts a new chat session for the user.
        /// Auto-closes any existing active session before creating a new one.
        /// Validates user consent (AcceptedAiChat) before proceeding.
        /// </summary>
        Task<SessionDTO> StartSessionAsync(string userId, StartSessionDTO dto);

        /// <summary>
        /// Ends an active session. Validates ownership before updating status.
        /// </summary>
        Task<SessionDTO> EndSessionAsync(string userId, Guid sessionId);

        /// <summary>
        /// Returns paginated session history for a user, newest first.
        /// </summary>
        Task<PaginatedSessionsDTO> GetUserSessionsAsync(
            string userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Returns a specific session with its complete message history.
        /// Returns null if the session does not exist or does not belong to the user.
        /// </summary>
        Task<ChatHistoryDTO?> GetSessionHistoryAsync(string userId, Guid sessionId);

        /// <summary>
        /// Processes a user message: saves it, calls the AI, saves the response,
        /// handles risk detection, and returns the complete exchange.
        /// </summary>
        Task<SendMessageResponseDTO> SendMessageAsync(string userId, UserChatRequestDto dto);

        /// <summary>
        /// Soft-deletes a session (marks IsDeleted = true via EF Core interceptor).
        /// Validates ownership before deletion.
        /// </summary>
        Task<bool> DeleteSessionAsync(string userId, Guid sessionId);
    }
}
