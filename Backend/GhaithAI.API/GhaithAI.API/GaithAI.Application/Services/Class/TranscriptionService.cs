using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using System.Net.Http.Headers;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class TranscriptionService : ITranscriptionService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HttpClient _httpClient;
        private readonly string _subscriptionKey;
        private readonly string _region;

        public TranscriptionService(
            IServiceScopeFactory scopeFactory,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _httpClient = httpClient;
            _subscriptionKey = configuration["AzureSpeech:SubscriptionKey"]
                ?? throw new InvalidOperationException("AzureSpeech:SubscriptionKey is missing from appsettings.json");
            _region = configuration["AzureSpeech:Region"]
                ?? throw new InvalidOperationException("AzureSpeech:Region is missing from appsettings.json");
        }

        public async Task ProcessAsync(Guid clinicalSessionId, string audioFilePath)
        {
            try
            {
                var transcriptionUrl = await UploadAndStartTranscriptionAsync(audioFilePath);
                var segments = await PollUntilCompletedAsync(transcriptionUrl);
                await SaveSegmentsAsync(clinicalSessionId, segments);
                await UpdateSessionStatusAsync(clinicalSessionId, TranscriptionStatus.Completed);
            }
            catch
            {
                await UpdateSessionStatusAsync(clinicalSessionId, TranscriptionStatus.Failed);
            }
            finally
            {
                if (File.Exists(audioFilePath))
                    File.Delete(audioFilePath);
            }
        }

        private async Task<string> UploadAndStartTranscriptionAsync(string audioFilePath)
        {
            var baseUrl = $"https://{_region}.api.cognitive.microsoft.com/speechtotext/v3.1/transcriptions";

            var requestBody = new
            {
                contentUrls = Array.Empty<string>(),
                locale = "ar-EG",
                displayName = $"session-{Path.GetFileNameWithoutExtension(audioFilePath)}",
                properties = new
                {
                    diarizationEnabled = true,
                    wordLevelTimestampsEnabled = false,
                    punctuationMode = "DictatedAndAutomatic",
                    profanityFilterMode = "None"
                }
            };

            // Upload audio as multipart
            using var fileStream = File.OpenRead(audioFilePath);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");
            content.Add(fileContent, "audio", Path.GetFileName(audioFilePath));

            // First upload audio to get a content URL (simplified: use local file URI for batch API)
            // For Azure batch transcription, audio must be accessible via URL (blob storage).
            // Here we POST the transcription job with the file bytes inline via the files endpoint.
            var filesUrl = $"https://{_region}.api.cognitive.microsoft.com/speechtotext/v3.1/transcriptions:transcribe";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

            var jsonBody = JsonSerializer.Serialize(new
            {
                locale = "ar-EG",
                diarizationEnabled = true
            });

            using var formData = new MultipartFormDataContent();
            var audioBytes = await File.ReadAllBytesAsync(audioFilePath);
            var audioContent = new ByteArrayContent(audioBytes);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");
            formData.Add(audioContent, "audio", Path.GetFileName(audioFilePath));
            formData.Add(new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json"), "definition");

            var response = await _httpClient.PostAsync(filesUrl, formData);
            response.EnsureSuccessStatusCode();

            var locationHeader = response.Headers.Location?.ToString()
                ?? throw new InvalidOperationException("Azure Speech did not return a transcription job URL.");

            return locationHeader;
        }

        private async Task<List<TranscriptSegmentRaw>> PollUntilCompletedAsync(string transcriptionUrl)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(30));

                var response = await _httpClient.GetAsync(transcriptionUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var status = root.GetProperty("status").GetString();

                if (status == "Succeeded")
                {
                    var filesUrl = root.GetProperty("links").GetProperty("files").GetString();
                    return await FetchSegmentsAsync(filesUrl);
                }

                if (status == "Failed")
                    throw new InvalidOperationException("Azure Speech transcription job failed.");
            }
        }

        private async Task<List<TranscriptSegmentRaw>> FetchSegmentsAsync(string filesUrl)
        {
            var response = await _httpClient.GetAsync(filesUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var resultUrl = doc.RootElement
                .GetProperty("values")
                .EnumerateArray()
                .First(v => v.GetProperty("kind").GetString() == "Transcription")
                .GetProperty("links")
                .GetProperty("contentUrl")
                .GetString();

            var resultResponse = await _httpClient.GetAsync(resultUrl);
            resultResponse.EnsureSuccessStatusCode();

            var resultJson = await resultResponse.Content.ReadAsStringAsync();
            using var resultDoc = JsonDocument.Parse(resultJson);

            var segments = new List<TranscriptSegmentRaw>();

            foreach (var phrase in resultDoc.RootElement
                .GetProperty("recognizedPhrases")
                .EnumerateArray())
            {
                var speakerLabel = phrase.TryGetProperty("speaker", out var sp) ? sp.GetInt32() : 0;
                var best = phrase.GetProperty("nBest")[0];

                segments.Add(new TranscriptSegmentRaw
                {
                    Speaker = speakerLabel == 1 ? SpeakerRole.Doctor : SpeakerRole.Patient,
                    Content = best.GetProperty("display").GetString() ?? string.Empty,
                    StartMs = phrase.GetProperty("offsetInTicks").GetInt64() / 10000,
                    EndMs = (phrase.GetProperty("offsetInTicks").GetInt64()
                             + phrase.GetProperty("durationInTicks").GetInt64()) / 10000,
                    ConfidenceScore = best.TryGetProperty("confidence", out var conf)
                        ? (decimal?)conf.GetDouble()
                        : null
                });
            }

            return segments;
        }

        private async Task SaveSegmentsAsync(Guid clinicalSessionId, List<TranscriptSegmentRaw> segments)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var entities = segments.Select(s => new SessionTranscript
            {
                Id = Guid.NewGuid(),
                ClinicalSessionId = clinicalSessionId,
                Speaker = s.Speaker,
                Content = s.Content,
                StartMs = (int)s.StartMs,
                EndMs = (int)s.EndMs,
                ConfidenceScore = s.ConfidenceScore,
                IsEdited = false
            }).ToList();

            await unitOfWork.SessionTranscript.AddRangeAsync(entities);
            await unitOfWork.CompleteAsync();
        }

        private async Task UpdateSessionStatusAsync(Guid clinicalSessionId, TranscriptionStatus status)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var session = await unitOfWork.ClinicalSession
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(s => s.Id == clinicalSessionId);

            if (session == null) return;

            session.TranscriptionStatus = status;
            await unitOfWork.CompleteAsync();
        }

        private class TranscriptSegmentRaw
        {
            public SpeakerRole Speaker { get; set; }
            public string Content { get; set; }
            public long StartMs { get; set; }
            public long EndMs { get; set; }
            public decimal? ConfidenceScore { get; set; }
        }
    }
}
