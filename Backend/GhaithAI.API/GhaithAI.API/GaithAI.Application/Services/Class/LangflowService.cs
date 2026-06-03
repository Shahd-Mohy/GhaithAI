global using GhaithAI.API.GaithAI.Application.DTOs.Chat;
global using System.Text;
global using System.Text.Json;
using GhaithAI.API.GaithAI.API.Configurations;
using Microsoft.Extensions.Options;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    public class LangflowService : ILangflowService
    {
        private readonly HttpClient _httpClient;
        private readonly LangflowSettings _langflowSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private const string AgentNodeId = "Prompt-Agent-xUYMm";

        public LangflowService(
            HttpClient httpClient,
            IOptions<LangflowSettings> langflowSettings,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _httpClient = httpClient;
            _langflowSettings = langflowSettings.Value;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<string> SendMessageAsync(string userMessage, string sessionId)
        {
            var request = new
            {
                input_value = userMessage,
                session_id = sessionId,
                output_type = "chat",
                input_type = "chat"
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _langflowSettings.ApiKey);

            var response = await _httpClient.PostAsJsonAsync(
                $"{_langflowSettings.BaseUrl}/api/v1/run/{_langflowSettings.FlowId}?stream=false",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Langflow Error: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<LangflowResponse>();

            return result!.Outputs[0].Outputs[0].Results.Message.Text;
        }

        public async Task<GhaithFinalResultDto> ProcessUserMessageAsync(Guid sessionId, string userMessage)
        {
            string conversationHistory = await _unitOfWork.Chat.GetLast30MessagesFormattedAsync(sessionId);

            var requestBody = new LangflowRequestDto
            {
                InputValue = userMessage,
                SessionId = sessionId.ToString(),
                Tweaks = new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        AgentNodeId, new Dictionary<string, string>
                        {
                            { "CONVERSATION_HISTORY_PLACEHOLDER", conversationHistory }
                        }
                    }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var requestUrl = $"{_langflowSettings.BaseUrl.TrimEnd('/')}/api/v1/run/{_langflowSettings.FlowId}?stream=false";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = content };
            request.Headers.Add("x-api-key", _langflowSettings.ApiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var rawText = root.GetProperty("outputs").EnumerateArray().First()
                              .GetProperty("outputs").EnumerateArray().First()
                              .GetProperty("results")
                              .GetProperty("message")
                              .GetProperty("text")
                              .GetString() ?? string.Empty;

            rawText = rawText.Trim();

            var finalResult = new GhaithFinalResultDto();

            if (rawText.StartsWith("{") && rawText.EndsWith("}"))
            {
                try
                {
                    var internalJson = JsonSerializer.Deserialize<GhaithAiInternalResponseDto>(rawText);
                    if (internalJson != null)
                    {
                        finalResult.AiResponse = internalJson.AiResponse;
                        finalResult.IsRiskDetected = internalJson.IsRiskDetected;
                        finalResult.RiskDetails = internalJson.RiskDetails;
                    }
                }
                catch
                {
                    finalResult.AiResponse = rawText;
                    finalResult.IsRiskDetected = false;
                }
            }
            else
            {
                finalResult.AiResponse = rawText;
                finalResult.IsRiskDetected = false;
            }

            var userMessageEntity = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                SenderType = "User",
                Content = userMessage
            };

            var aiMessageEntity = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                SenderType = "AI",
                Content = finalResult.AiResponse
            };

            await _unitOfWork.Message.AddAsync(userMessageEntity);
            await _unitOfWork.Message.AddAsync(aiMessageEntity);

            if (finalResult.IsRiskDetected && finalResult.RiskDetails != null)
            {
                var riskEntity = _mapper.Map<RiskEvent>(finalResult.RiskDetails);
                riskEntity.SessionId = sessionId;
                riskEntity.MessageId = userMessageEntity.Id;

                await _unitOfWork.Risk.AddAsync(riskEntity);
            }

            await _unitOfWork.CompleteAsync();

            return finalResult;
        }
    }

    public class LangflowResponse
    {
        [JsonPropertyName("outputs")]
        public List<OutputItem> Outputs { get; set; } = new();
    }

    public class OutputItem
    {
        [JsonPropertyName("outputs")]
        public List<OutputData> Outputs { get; set; } = new();
    }

    public class OutputData
    {
        [JsonPropertyName("results")]
        public ResultData Results { get; set; } = new();
    }

    public class ResultData
    {
        [JsonPropertyName("message")]
        public MessageData Message { get; set; } = new();
    }

    public class MessageData
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}