namespace GhaithAI.API.GaithAI.Application.DTOs.Chat
{
    public class GhaithFinalResultDto
    {
        public string AiResponse { get; set; } = string.Empty;
        public bool IsRiskDetected { get; set; }
        public RiskDetailsDto? RiskDetails { get; set; }
    }
}
