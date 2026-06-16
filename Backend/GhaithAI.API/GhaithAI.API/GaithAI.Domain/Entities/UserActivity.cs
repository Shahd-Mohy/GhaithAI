using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents a user's activity log for self-help content.
    /// Immutable transactional log entry capturing point-in-time user activity.
    /// </summary>
    public class UserActivity : BaseEntity<Guid>
    {
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public Guid ContentId { get; set; }

        public SelfHelpContent SelfHelpContent { get; set; }

        public string ActivityType { get; set; }

        public int DurationSeconds { get; set; } = 0;

        public int ProgressPercent { get; set; }

        public bool Completed { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public string? Metadata { get; set; }
    }
}