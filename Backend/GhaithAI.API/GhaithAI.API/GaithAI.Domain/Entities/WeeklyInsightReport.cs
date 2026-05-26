using System.ComponentModel.DataAnnotations;

namespace GhaithAI.API.Models
{
    public class WeeklyInsightReport
    {
        [Key]   
        public Guid ReportId { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public decimal? AvgMoodScore { get; set; }

        public string? BestDay { get; set; }

        public string? ToughestDay { get; set; }

        public string? DetectedPatterns { get; set; }

        public string? InsightSummary { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}