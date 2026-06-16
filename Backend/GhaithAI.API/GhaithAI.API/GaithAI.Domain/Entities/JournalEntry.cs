using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents a user's journal entry.
    /// Sensitive clinical entity requiring full audit trail and soft delete capabilities.
    /// </summary>
    public class JournalEntry : AuditableEntity<Guid>
    {
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string PromptType { get; set; } = "free";

        public string? Title { get; set; }

        public string Content { get; set; }

        public int WordCount { get; set; } = 0;

        public string MoodBefore { get; set; } = string.Empty;

        public string MoodAfter { get; set; } = string.Empty;

        public string? Tags { get; set; }
    }
}