using System.ComponentModel.DataAnnotations;

namespace GhaithAI.API.Models
{
    public class MoodLog
    {
        [Key]
        public Guid MoodLogId { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public int MoodScore { get; set; }

        public string? EmotionTags { get; set; }

        public int StressLevel { get; set; }

        public int SleepQuality { get; set; }

        public string? Notes { get; set; }

        public string Source { get; set; } = "manual";

        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    }
}