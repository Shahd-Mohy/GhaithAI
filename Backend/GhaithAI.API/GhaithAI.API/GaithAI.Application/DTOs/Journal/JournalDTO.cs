namespace GhaithAI.API.DTOs.Journal
{
    public class JournalDTO
    {
        public Guid JournalId { get; set; }

        public string? Title { get; set; }

        public string ContentPreview { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string PromptType { get; set; } = string.Empty;

        public string MoodBefore { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new();

        public int WordCount { get; set; }

        public string Date { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
