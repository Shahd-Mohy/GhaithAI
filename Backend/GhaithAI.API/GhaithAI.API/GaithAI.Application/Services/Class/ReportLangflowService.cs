using GhaithAI.API.GaithAI.API.Configurations;
using GhaithAI.API.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    /// <summary>
    /// Handles all HTTP communication with the Langflow 3-stage clinical report pipeline.
    /// Single responsibility: send a transcript → receive and parse the AI JSON response.
    /// </summary>
    public sealed class ReportLangflowService : IReportLangflowService
    {
        private readonly HttpClient _httpClient;
        private readonly LangflowReportSettings _settings;
        private readonly ILogger<ReportLangflowService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions =
            new() { PropertyNameCaseInsensitive = true };

        public ReportLangflowService(
            HttpClient httpClient,
            IOptions<LangflowReportSettings> settings,
            ILogger<ReportLangflowService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ReportLangflowResult> AnalyzeTranscriptAsync(
            string transcriptText,
            Guid sessionId)
        {
            // ── 1. Build request ──────────────────────────────────────────────
            var requestUrl =
                $"{_settings.BaseUrl.TrimEnd('/')}/api/v1/run/{_settings.FlowId}?stream=false";

            var body = new
            {
                output_type = "chat",
                input_type  = "chat",
                session_id  = sessionId.ToString(),
                tweaks = new Dictionary<string, object>
                {
                    [_settings.ChatInputNodeId]        = new { input_value = transcriptText },
                    [_settings.JsonCompilerNodeId]     = new { max_tokens = 25000, temperature = 0 },
                    [_settings.ClinicalAnalystNodeId]  = new { max_tokens = 16000, temperature = 0.07 },
                    [_settings.MedicalComplianceNodeId]= new { max_tokens = 16000, temperature = 0.01 }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = JsonContent.Create(body)
            };
            request.Headers.Add("x-api-key", _settings.ApiKey);

            _logger.LogInformation(
                "Calling Langflow report pipeline for session {SessionId} ({Chars} chars)",
                sessionId, transcriptText.Length);

            // ── 2. Send ───────────────────────────────────────────────────────
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Langflow report pipeline returned {Code} for session {SessionId}. Body: {Body}",
                    (int)response.StatusCode, sessionId, error);
                throw new HttpRequestException(
                    $"Langflow report pipeline returned {(int)response.StatusCode}: {error}");
            }

            // ── 3. Extract text from envelope ─────────────────────────────────
            var rawText = await ExtractTextAsync(response, sessionId);

            // ── 4. Parse into typed DTO ───────────────────────────────────────
            var parsed = DeserializeOutput(rawText, sessionId);

            return new ReportLangflowResult(parsed, rawText);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Navigates the Langflow response envelope:
        ///   outputs[0].outputs[0].results.message.text
        /// and strips any Markdown code-fence wrapper.
        /// </summary>
        private async Task<string> ExtractTextAsync(HttpResponseMessage response, Guid sessionId)
        {
            var json = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(json);

                var text = doc.RootElement
                    .GetProperty("outputs")[0]
                    .GetProperty("outputs")[0]
                    .GetProperty("results")
                    .GetProperty("message")
                    .GetProperty("text")
                    .GetString() ?? string.Empty;

                return StripMarkdownFence(text.Trim());
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex,
                    "Failed to parse Langflow envelope for session {SessionId}. Raw: {Raw}",
                    sessionId, json);
                throw new InvalidOperationException(
                    "Failed to parse Langflow report envelope. See inner exception.", ex);
            }
        }

        /// <summary>
        /// Deserializes the cleaned JSON text into <see cref="ReportAiOutputDto"/>.
        /// </summary>
        private ReportAiOutputDto DeserializeOutput(string cleanJson, Guid sessionId)
        {
            try
            {
                var dto = JsonSerializer.Deserialize<ReportAiOutputDto>(cleanJson, _jsonOptions)
                    ?? throw new InvalidOperationException("Deserialized AI output was null.");
                return dto;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Failed to deserialize AI report JSON for session {SessionId}. Raw: {Raw}",
                    sessionId, cleanJson);
                throw new InvalidOperationException(
                    "The AI returned a response that could not be parsed as a clinical report JSON.",
                    ex);
            }
        }

        /// <summary>
        /// Removes Markdown code-fence wrappers (e.g. ```json … ```) that the AI
        /// may include around its JSON output.
        /// </summary>
        private static string StripMarkdownFence(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var t = text.Trim();
            if (!t.StartsWith("```") || !t.EndsWith("```")) return t;

            var nl = t.IndexOf('\n');
            if (nl < 0) return t;

            var openLine = t[..nl].TrimEnd('\r');
            if (!Regex.IsMatch(openLine, "^```[a-zA-Z]*$")) return t;

            var inner = t[(nl + 1)..];
            var closing = inner.LastIndexOf("```", StringComparison.Ordinal);
            return closing < 0 ? t : inner[..closing].TrimEnd('\r', '\n').Trim();
        }
    }
}
