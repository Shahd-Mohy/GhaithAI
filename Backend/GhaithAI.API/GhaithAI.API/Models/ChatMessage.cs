using System.ComponentModel.DataAnnotations;

namespace GhaithAI.API.Models
{
    public class ChatMessage
    {
        [Key]
        public Guid MessageId { get; set; }

        public Guid SessionId { get; set; }

        public ChatSession ChatSession { get; set; }

        public string SenderType { get; set; }

        public string Content { get; set; }

        public decimal? SentimentScore { get; set; }

        public string? DetectedEmotion { get; set; }

        public string? DetectedLanguage { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public ICollection<RiskEvent> RiskEvents { get; set; }
    }
}