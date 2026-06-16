namespace GhaithAI.API.DTOs.Chat
{
    public class SessionDTO
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;       // active | ended
        public string RiskLevel { get; set; } = string.Empty;    // low | medium | high
        public bool MemoryEnabled { get; set; }
        public string? Title { get; set; }
        public string? EmotionalTone { get; set; }
        public string? AISummary { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int MessageCount { get; set; }
    }
}
