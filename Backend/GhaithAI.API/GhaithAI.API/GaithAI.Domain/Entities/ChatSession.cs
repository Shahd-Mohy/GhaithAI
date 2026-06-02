using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents a chat session between a user and the AI assistant.
    /// Sensitive clinical entity requiring full audit trail and soft delete capabilities.
    /// </summary>
    public class ChatSession : AuditableEntity<Guid>
    {
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public DateTime? EndedAt { get; set; }

        public string Status { get; set; } = "active";

        public string RiskLevel { get; set; } = "low";

        public string? AISummary { get; set; }

        public string? EmotionalTone { get; set; }

        public string? MoodChange { get; set; }

        public bool MemoryEnabled { get; set; } = false;

        public string? Title { get; set; }

        public ICollection<ChatMessage> ChatMessages { get; set; }

        public ICollection<RiskEvent> RiskEvents { get; set; }
    }
}