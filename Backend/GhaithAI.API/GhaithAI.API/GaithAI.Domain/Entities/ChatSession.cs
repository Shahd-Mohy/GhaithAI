using System.ComponentModel.DataAnnotations;

namespace GhaithAI.API.Models
{
    public class ChatSession
    {
        [Key]
        public Guid SessionId { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndedAt { get; set; }

        public string Status { get; set; } = "active";

        public string RiskLevel { get; set; } = "low";

        public string? AISummary { get; set; }

        public string? EmotionalTone { get; set; }

        public string? MoodChange { get; set; }

        public bool MemoryEnabled { get; set; } = false;

        public string Title { get; set; } = string.Empty;

        public ICollection<ChatMessage> ChatMessages { get; set; }

        public ICollection<RiskEvent> RiskEvents { get; set; }
    }
}