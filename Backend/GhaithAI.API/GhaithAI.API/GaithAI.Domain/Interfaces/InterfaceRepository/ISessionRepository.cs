using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.Models;

namespace GhaithAI.API.Repositories.Interfaces
{
    /// <summary>
    /// Specialized repository for ChatSession lifecycle management.
    /// Extends generic CRUD with session-specific queries required by the Personal Support Module.
    /// </summary>
    public interface ISessionRepository : IGenericRepository<ChatSession>
    {
        /// <summary>
        /// Retrieves the last 30 messages of a session formatted as conversation history.
        /// Used by LangflowService to build the AI context prompt.
        /// </summary>
        Task<string> GetLast30MessagesFormattedAsync(Guid sessionId);

        /// <summary>
        /// Retrieves the currently active session for a user (status = "active").
        /// A user can only have one active session at a time 
        /// Returns null if no active session exists.
        /// </summary>
        Task<ChatSession?> GetActiveSessionAsync(string userId);

        /// <summary>
        /// Retrieves a paginated list of all sessions for a user, ordered by newest first.
        /// Returns both the page items and the total count for pagination metadata.
        /// </summary>
        Task<(IEnumerable<ChatSession> Items, int TotalCount)> GetUserSessionsAsync(
            string userId, int page, int pageSize);

        /// <summary>
        /// Retrieves a session with its full message history (ordered chronologically).
        /// Enforces ownership — only returns if the session belongs to the given user.
        /// Returns null if not found or ownership mismatch.
        /// </summary>
        Task<ChatSession?> GetSessionWithMessagesAsync(Guid sessionId, string userId);

        /// <summary>
        /// Lightweight ownership check — does not load the full entity.
        /// Use before performing updates or deletes to prevent IDOR attacks.
        /// </summary>
        Task<bool> BelongsToUserAsync(Guid sessionId, string userId);
    }
}
