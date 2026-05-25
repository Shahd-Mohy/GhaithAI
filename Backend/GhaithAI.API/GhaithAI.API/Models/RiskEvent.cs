using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GhaithAI.API.Models
{
    public class RiskEvent
    {
        [Key]
        public Guid RiskId { get; set; }

        public Guid SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public ChatSession ChatSession { get; set; }

        public Guid MessageId { get; set; }

        [ForeignKey(nameof(MessageId))]
        public ChatMessage ChatMessage { get; set; }

        public string RiskType { get; set; }

        public string? DetectedMarkers { get; set; }

        public string? SupportingContext { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public string? AIActionTaken { get; set; }

        public string Status { get; set; } = "open";

        public bool IsDeletable { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}