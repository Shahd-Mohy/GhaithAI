using System.Text.Json.Serialization;

namespace GhaithAI.API.GaithAI.Application.DTOs.Chat
{
    public class LangflowRequestDto
    {
        [JsonPropertyName("input_value")]
        public string InputValue { get; set; } = string.Empty;

        [JsonPropertyName("input_type")]
        public string InputType { get; set; } = "chat";

        [JsonPropertyName("output_type")]
        public string OutputType { get; set; } = "chat";

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = string.Empty;

        [JsonPropertyName("tweaks")]
        public Dictionary<string, Dictionary<string, string>> Tweaks { get; set; } = new();
    }
}
