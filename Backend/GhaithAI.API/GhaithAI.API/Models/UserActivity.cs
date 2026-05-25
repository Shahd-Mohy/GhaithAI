using System.ComponentModel.DataAnnotations;

namespace GhaithAI.API.Models
{
    public class UserActivity
    {
        [Key]
        public Guid ActivityId { get; set; }

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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}