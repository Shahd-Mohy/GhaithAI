using GhaithAI.API.GaithAI.Application.DTOs.Chat;

namespace GhaithAI.API.DTOs.Chat
{
    public class SendMessageResponseDTO
    {
        public ChatMessageDTO UserMessage { get; set; } = null!;
        public ChatMessageDTO AiMessage { get; set; } = null!;
        public bool IsRiskDetected { get; set; }
        public RiskDetailsDto? RiskDetails { get; set; }
    }
}
