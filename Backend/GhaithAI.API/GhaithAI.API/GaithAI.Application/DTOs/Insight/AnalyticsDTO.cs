namespace GhaithAI.API.DTOs.Insight
{
    public class AnalyticsDTO
    {
        public double AvgMood { get; set; }

        public int DaysLogged { get; set; }

        public int CurrentStreak { get; set; }

        public string? TopEmotion { get; set; }

        public double? MoodChangeFromLastWeek { get; set; }
    }
}
