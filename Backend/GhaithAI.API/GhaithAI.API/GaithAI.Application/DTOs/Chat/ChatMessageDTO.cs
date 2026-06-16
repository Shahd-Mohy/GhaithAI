namespace GhaithAI.API.GaithAI.Application.DTOs.Chat
{
    public class ChatMessageDTO
    {
        public Guid Id { get; set; }
        public string SenderType { get; set; } = string.Empty;   // User | AI
        public string Content { get; set; } = string.Empty;
        public decimal? SentimentScore { get; set; }
        public string? DetectedEmotion { get; set; }
        public string? DetectedLanguage { get; set; }
        public DateTime SentAt { get; set; }
    }
}
