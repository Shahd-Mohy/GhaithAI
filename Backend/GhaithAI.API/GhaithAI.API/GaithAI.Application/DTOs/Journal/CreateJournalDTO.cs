namespace GhaithAI.API.DTOs.Journal
{
    public class CreateJournalDTO
    {
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;

        public string PromptType { get; set; } = "free";

        public string MoodBefore { get; set; } = string.Empty;
        public string? Tags { get; set; }
    }
}
