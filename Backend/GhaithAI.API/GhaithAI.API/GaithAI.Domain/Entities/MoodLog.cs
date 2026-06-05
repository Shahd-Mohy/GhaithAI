using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    public class MoodLog : BaseEntity<Guid>
    {
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int MoodScore { get; set; }
        public string? EmotionTags { get; set; }
        public int StressLevel { get; set; }
        public int SleepQuality { get; set; }
        public string? Notes { get; set; }
        public string Source { get; set; } = "manual";
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}