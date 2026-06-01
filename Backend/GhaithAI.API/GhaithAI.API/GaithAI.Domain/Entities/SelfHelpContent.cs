using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents self-help content available to users.
    /// Sensitive clinical entity requiring full audit trail and soft delete capabilities.
    /// </summary>
    public class SelfHelpContent : AuditableEntity<Guid>
    {
        public string Title { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public string? ContentUrl { get; set; }

        public int? DurationMinutes { get; set; }

        public string DifficultyLevel { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<UserActivity> UserActivities { get; set; }
    }
}