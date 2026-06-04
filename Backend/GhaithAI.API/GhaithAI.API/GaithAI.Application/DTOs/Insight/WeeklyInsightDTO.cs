using GhaithAI.GaithAI.Application.DTOs.Insight;

namespace GhaithAI.API.DTOs.Insight
{
    public class WeeklyInsightDTO
    {
        public List<DailyMoodDTO> DailyMoods { get; set; } = new();

        public decimal AvgMoodScore { get; set; }

        public string AvgMoodLabel { get; set; } = string.Empty;

        public int DaysLogged { get; set; }

        public string DaysLoggedLabel { get; set; } = string.Empty;


    }


}
