using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents a weekly insight report generated for a user.
    /// Immutable read-only report capturing point-in-time analytics and insights.
    /// </summary>
    public class WeeklyInsightReport : BaseEntity<Guid>
    {
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public decimal? AvgMoodScore { get; set; }

        public string? BestDay { get; set; }

        public string? ToughestDay { get; set; }

        public string? DetectedPatterns { get; set; }

        public string? InsightSummary { get; set; }
    }
}