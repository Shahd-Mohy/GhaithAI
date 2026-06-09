using GhaithAI.API.GaithAI.API.Configurations;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    public sealed class LangflowService : ILangflowService
    {
        private readonly HttpClient _httpClient;
        private readonly LangflowSettings _langflowSettings;
        private readonly ILogger<LangflowService> _logger;

        public LangflowService(
            HttpClient httpClient,
            IOptions<LangflowSettings> langflowSettings,
            ILogger<LangflowService> logger)
        {
            _httpClient = httpClient;
            _langflowSettings = langflowSettings.Value;
            _logger = logger;
        }

        public async Task<GhaithFinalResultDto> ProcessUserMessageAsync(
            Guid sessionId,
            string userMessage,
            AiContextPackageDto contextPackage)
        {
            // ── 1. Build request
            var requestBody = new LangflowRequestDto
            {
                InputValue = userMessage,
                SessionId = sessionId.ToString(),
                Tweaks = new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        _langflowSettings.AgentNodeId,
                        new Dictionary<string, string>
                        {
                            { _langflowSettings.ConversationHistoryPlaceholder, contextPackage.ConversationHistory },
                            { _langflowSettings.UserContextPlaceholder,         FormatUserContext(contextPackage.UserContext) },
                            { _langflowSettings.MoodContextPlaceholder,         FormatMoodContext(contextPackage.MoodContext) }
                        }
                    }
                }
            };

            var requestUrl = $"{_langflowSettings.BaseUrl.TrimEnd('/')}/api/v1/run/{_langflowSettings.FlowId}?stream=false";

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Add("x-api-key", _langflowSettings.ApiKey);

            // ── 2. Send & validate
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Langflow returned {StatusCode} for session {SessionId}. Body: {ErrorBody}",
                    (int)response.StatusCode, sessionId, errorBody);

                throw new HttpRequestException($"Langflow service returned {(int)response.StatusCode}: {errorBody}");
            }

            // ── 3. Parse response
            var rawText = await ExtractTextFromResponseAsync(response, sessionId);

            // ── 4. Detect structured risk payload
            return ParseAiResult(rawText, sessionId);
        }

        private static string FormatUserContext(AiUserContextDto user)
        {
            if (user is null) return "User context unavailable.";

            return $"""
                [USER PROFILE]
                Name: {user.FullName}
                Preferred Language: "Arabic"
                Country: {user.CountryCode}
                Memory Enabled: {(user.MemoryEnabled ? "Yes" : "No")}
                """;
        }

        private static string FormatMoodContext(AiMoodContextDto mood)
        {
            if (mood is null || mood.HasNoData)
                return "Mood context: No mood data available for the last 7 days.";

            var emotionLine = mood.EmotionPattern.Any()
                ? string.Join(", ", mood.EmotionPattern)
                : "None recorded";

            return $"""
                [MOOD CONTEXT — Last 7 Days]
                Average Mood Score: {mood.AverageMoodScore}/10 ({mood.AverageMoodLabel})
                Mood Trend: {mood.MoodTrend} ({(mood.ChangeFromPreviousWeek >= 0 ? "+" : "")}{mood.ChangeFromPreviousWeek} vs previous week)
                Days Logged: {mood.LoggedDaysCount}/7
                Current Streak: {mood.StreakDays} day(s)
                Dominant Emotion: {mood.DominantEmotion} (appeared {mood.DominantEmotionCount} time(s))
                Emotion Pattern: {emotionLine}
                """;
        }

        private async Task<string> ExtractTextFromResponseAsync(HttpResponseMessage response, Guid sessionId)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                if (!root.TryGetProperty("outputs", out var outputs) || outputs.ValueKind != JsonValueKind.Array)
                    throw new InvalidOperationException("Missing 'outputs' array in Langflow response.");

                var firstOutput = outputs.EnumerateArray().FirstOrDefault();
                if (firstOutput.ValueKind == JsonValueKind.Undefined)
                    throw new InvalidOperationException("Langflow 'outputs' array is empty.");

                if (!firstOutput.TryGetProperty("outputs", out var innerOutputs) || innerOutputs.ValueKind != JsonValueKind.Array)
                    throw new InvalidOperationException("Missing nested 'outputs' array.");

                var firstInner = innerOutputs.EnumerateArray().FirstOrDefault();
                if (firstInner.ValueKind == JsonValueKind.Undefined)
                    throw new InvalidOperationException("Langflow nested 'outputs' array is empty.");

                var text = firstInner
                    .GetProperty("results")
                    .GetProperty("message")
                    .GetProperty("text")
                    .GetString() ?? string.Empty;

                return text.Trim();
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex,
                    "Failed to parse Langflow response for session {SessionId}. Raw: {Raw}",
                    sessionId, jsonResponse);

                throw new InvalidOperationException("Failed to parse Langflow response. See inner exception.", ex);
            }
        }

        /// <summary>
        /// Strips any Markdown code-fence wrapper that Langflow may add around the
        /// JSON payload (e.g. ```json ... ```) and then attempts to deserialise
        /// the cleaned text as <see cref="GhaithAiInternalResponseDto"/>.
        ///
        /// Three cases handled:
        ///   1. Fenced JSON  →  ```json\n{...}\n```  →  stripped → deserialised
        ///   2. Bare JSON    →  {AiResponse:...}      →  deserialised directly
        ///   3. Plain text   →  anything else         →  returned as-is
        /// </summary>
        private GhaithFinalResultDto ParseAiResult(string rawText, Guid sessionId)
        {
            // ── 1. Strip Markdown code-fence (```json ... ``` or ``` ... ```) ──
            var cleaned = StripMarkdownCodeFence(rawText);

            // ── 2. Fast-path: not JSON-shaped → return as plain AI text ──────
            if (!cleaned.StartsWith('{') || !cleaned.EndsWith('}'))
            {
                return new GhaithFinalResultDto
                {
                    AiResponse = cleaned,
                    IsRiskDetected = false
                };
            }

            // ── 3. Attempt structured deserialization ─────────────────────────
            try
            {
                var structured = JsonSerializer.Deserialize<GhaithAiInternalResponseDto>(
                    cleaned,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (structured is not null)
                {
                    return new GhaithFinalResultDto
                    {
                        AiResponse = structured.AiResponse,
                        IsRiskDetected = structured.IsRiskDetected,
                        RiskDetails = structured.RiskDetails
                    };
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex,
                    "AI response for session {SessionId} looked like JSON but failed to deserialise as GhaithAiInternalResponseDto. Treating as plain text.",
                    sessionId);
            }

            // ── 4. JSON parse failed → return cleaned text as-is ─────────────
            return new GhaithFinalResultDto
            {
                AiResponse = cleaned,
                IsRiskDetected = false
            };
        }

        /// <summary>
        /// Removes a leading Markdown code-fence (``` or ```json / ```JSON)
        /// and its matching trailing ```.
        ///
        /// Handles:
        ///   • ```json\n...\n```   (Langflow's most common output)
        ///   • ```\n...\n```       (fence without language specifier)
        ///   • No fence            (returned unchanged)
        ///
        /// Preserves any content that genuinely starts/ends with backticks
        /// but is not a well-formed fence pair.
        /// </summary>
        private static string StripMarkdownCodeFence(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var trimmed = text.Trim();

            // Must start with ``` and end with ```
            if (!trimmed.StartsWith("```") || !trimmed.EndsWith("```"))
                return trimmed;

            // Locate the end of the opening fence line (after the optional language specifier)
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline < 0)
                return trimmed; // Malformed single-line fence — leave as-is

            // Validate that the opening line is only a fence + optional language tag
            // e.g. "```json" or "```" — nothing else
            var openingLine = trimmed[..firstNewline].TrimEnd('\r');
            if (!Regex.IsMatch(openingLine, "^```[a-zA-Z]*$"))
                return trimmed;

            // Strip the opening fence line and the closing ```
            var afterOpen = trimmed[(firstNewline + 1)..];
            var lastFence = afterOpen.LastIndexOf("```", StringComparison.Ordinal);

            if (lastFence < 0)
                return trimmed; // No closing fence — leave as-is

            var innerContent = afterOpen[..lastFence].TrimEnd('\r', '\n').Trim();
            return innerContent;
        }
    }
}
