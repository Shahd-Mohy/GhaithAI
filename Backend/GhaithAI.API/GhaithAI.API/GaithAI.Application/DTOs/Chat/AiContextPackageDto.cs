namespace GhaithAI.API.GaithAI.Application.DTOs.Chat
{
    public sealed class AiContextPackageDto
    {
        public string ConversationHistory { get; set; } = string.Empty;
        public AiUserContextDto UserContext { get; set; } = new();
        public AiMoodContextDto MoodContext { get; set; } = new();
    }

    public sealed class AiUserContextDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public bool MemoryEnabled { get; set; }
    }

    public sealed class AiMoodContextDto
    {
        public int LoggedDaysCount { get; set; }
        public decimal AverageMoodScore { get; set; }
        public string AverageMoodLabel { get; set; } = "No data";
        public string MoodTrend { get; set; } = "Insufficient data";
        public decimal ChangeFromPreviousWeek { get; set; }
        public string DominantEmotion { get; set; } = "None";
        public int DominantEmotionCount { get; set; }
        public List<string> EmotionPattern { get; set; } = new();
        public int StreakDays { get; set; }
        public bool HasNoData => LoggedDaysCount == 0;
    }
}
