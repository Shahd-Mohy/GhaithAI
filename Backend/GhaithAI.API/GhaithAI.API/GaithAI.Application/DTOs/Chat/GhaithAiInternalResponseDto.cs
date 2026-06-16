
global using System.Text.Json.Serialization;

namespace GhaithAI.API.GaithAI.Application.DTOs.Chat
{
    public class GhaithAiInternalResponseDto
    {
        [JsonPropertyName("AiResponse")]
        public string AiResponse { get; set; } = string.Empty;

        [JsonPropertyName("IsRiskDetected")]
        public bool IsRiskDetected { get; set; }

        [JsonPropertyName("RiskDetails")]
        public RiskDetailsDto? RiskDetails { get; set; }
    }
}
