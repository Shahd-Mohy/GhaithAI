using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

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
                throw new Exception($"LangFlow request failed with status {response.StatusCode}");
            }

            var raw = await response.Content.ReadAsStringAsync(ct);

            // LangFlow بيرجع response object فيه outputs array — بنستخرج
            // منها الناتج الفعلي اللي الـ flow بناه (الـ JSON النهائي).
            using var doc = JsonDocument.Parse(raw);

            var outputText = doc.RootElement
                .GetProperty("outputs")[0]
                .GetProperty("outputs")[0]
                .GetProperty("results")
                .GetProperty("message")
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(outputText))
                throw new Exception("LangFlow returned an empty output.");

            return outputText;
        }
    }
}