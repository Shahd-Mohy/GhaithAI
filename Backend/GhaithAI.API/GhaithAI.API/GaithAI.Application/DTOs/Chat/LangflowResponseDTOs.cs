using System.Text.Json.Serialization;

namespace GhaithAI.API.GaithAI.Application.DTOs.Chat;

/// <summary>Root response envelope returned by the Langflow run endpoint.</summary>
internal sealed class LangflowResponse
{
    [JsonPropertyName("outputs")]
    public List<LangflowOutputItem> Outputs { get; set; } = new();
}

internal sealed class LangflowOutputItem
{
    [JsonPropertyName("outputs")]
    public List<LangflowOutputData> Outputs { get; set; } = new();
}

internal sealed class LangflowOutputData
{
    [JsonPropertyName("results")]
    public LangflowResultData Results { get; set; } = new();
}

internal sealed class LangflowResultData
{
    [JsonPropertyName("message")]
    public LangflowMessageData Message { get; set; } = new();
}

internal sealed class LangflowMessageData
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
