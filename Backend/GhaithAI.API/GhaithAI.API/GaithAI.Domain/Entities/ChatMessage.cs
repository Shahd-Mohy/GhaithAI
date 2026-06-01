using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents a single message within a chat session.
    /// Immutable transactional log entry that captures point-in-time communication.
    /// </summary>
    public class ChatMessage : BaseEntity<Guid>
    {
        public Guid SessionId { get; set; }

        public ChatSession ChatSession { get; set; }

        public string SenderType { get; set; }

        public string Content { get; set; }

        public decimal? SentimentScore { get; set; }

        public string? DetectedEmotion { get; set; }

        public string? DetectedLanguage { get; set; }

        public ICollection<RiskEvent> RiskEvents { get; set; }
    }
}