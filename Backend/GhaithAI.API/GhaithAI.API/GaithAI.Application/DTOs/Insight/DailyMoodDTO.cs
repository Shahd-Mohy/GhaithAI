namespace GhaithAI.GaithAI.Application.DTOs.Insight
{
    public class DailyMoodDTO
    {
            public string DayLabel { get; set; } = string.Empty;

            public string Date { get; set; } = string.Empty;

            public decimal? MoodScore { get; set; }
       
    }
}
