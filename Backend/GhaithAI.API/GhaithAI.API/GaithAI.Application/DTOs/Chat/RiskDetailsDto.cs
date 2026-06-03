
namespace GhaithAI.API.GaithAI.Application.DTOs.Chat
{
    public class RiskDetailsDto
    {
        [JsonPropertyName("RiskType")]
        public string RiskType { get; set; } = string.Empty;

        [JsonPropertyName("DetectedMarkers")]
        public string DetectedMarkers { get; set; } = string.Empty;

        [JsonPropertyName("ConfidenceScore")]
        public double ConfidenceScore { get; set; }

        [JsonPropertyName("SupportingContext")]
        public string SupportingContext { get; set; } = string.Empty;

        [JsonPropertyName("SuggestedAction")]
        public string SuggestedAction { get; set; } = string.Empty;
    }
}
