using GhaithAI.GaithAI.Domain.Exceptions;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class LangFlowClient : ILangFlowClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LangFlowClient> _logger;

        public LangFlowClient(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<LangFlowClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("LangFlowClient");
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> RunFlowAsync(string flowId, object payload, CancellationToken ct = default)
        {
            try
            {
                var requestBody = new
                {
                    input_value = JsonSerializer.Serialize(payload),
                    output_type = "chat",
                    input_type = "chat"
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/v1/run/{flowId}", requestBody, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("LangFlow call failed. FlowId={FlowId} Status={Status} Body={Body}",
                        flowId, response.StatusCode, errorBody);
                    throw new ExternalServiceException($"LangFlow request failed with status {response.StatusCode}.");
                }

                var raw = await response.Content.ReadAsStringAsync(ct);

                using var doc = JsonDocument.Parse(raw);

                var outputText = doc.RootElement
                    .GetProperty("outputs")[0]
                    .GetProperty("outputs")[0]
                    .GetProperty("results")
                    .GetProperty("message")
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(outputText))
                    throw new ExternalServiceException("LangFlow returned an empty output.");

                return outputText;
            }
            catch (ExternalServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LangFlow call failed for flow {FlowId}", flowId);
                throw new ExternalServiceException("The AI service failed while processing your request.");
            }
        }
    }
}