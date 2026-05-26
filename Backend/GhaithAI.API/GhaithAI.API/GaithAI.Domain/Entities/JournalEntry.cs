using System.ComponentModel.DataAnnotations;

namespace GhaithAI.API.Models
{
    public class JournalEntry
    {
        [Key]
        public Guid JournalId { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string PromptType { get; set; }

        public string? Title { get; set; }

        public string Content { get; set; }

        public int WordCount { get; set; } = 0;

        public int MoodBefore { get; set; }

        public int MoodAfter { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}