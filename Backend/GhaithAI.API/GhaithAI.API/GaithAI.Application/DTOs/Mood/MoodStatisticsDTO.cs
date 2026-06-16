namespace GhaithAI.API.DTOs.Mood
{
    public class MoodStatisticsDTO
    {
        public decimal AvgMoodScore { get; set; }

        public int TotalLogs { get; set; }
        public int StreakDays { get; set; }

        public string TopEmotion { get; set; } = string.Empty;

        public int TopEmotionCount { get; set; }

        public decimal ChangeFromLastWeek { get; set; }

        public string Period { get; set; } = string.Empty;
    }
}
