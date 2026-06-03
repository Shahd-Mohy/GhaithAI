
global using System.ComponentModel.DataAnnotations.Schema;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents a detected risk event in a chat session.
    /// Sensitive clinical entity requiring full audit trail and soft delete capabilities.
    /// </summary>
    public class RiskEvent : AuditableEntity<Guid>
    {
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
    }
}