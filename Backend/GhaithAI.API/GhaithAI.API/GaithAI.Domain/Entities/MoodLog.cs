using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents a mood log entry for a user.
    /// Immutable transactional log entry capturing point-in-time mood data.
    /// </summary>
    public class MoodLog : BaseEntity<Guid>
    {
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public int MoodScore { get; set; }

        public string? EmotionTags { get; set; }

        public int StressLevel { get; set; }

        public int SleepQuality { get; set; }

        public string? Notes { get; set; }

        public string Source { get; set; } = "manual";
    }
}