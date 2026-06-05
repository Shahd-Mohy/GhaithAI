using GhaithAI.API.DTOs.Insight;
using GhaithAI.GaithAI.Application.DTOs.Insight;

namespace GhaithAI.API.ViewModels
{
    public class DashboardViewModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public string TodayLabel { get; set; } = string.Empty;

        public WeeklyInsightDTO WeeklySummary { get; set; } = new();

        public List<PersonalInsightDTO> PersonalInsights { get; set; } = new();

        public RecommendationDTO TodaysSuggestion { get; set; } = new();
    }
}
