using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using System.Net.Http.Headers;
using System.Text;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class TranscriptionService : ITranscriptionService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TranscriptionService> _logger;
        private readonly string _apiKey;
        private readonly bool _isMockMode;

        private const string BaseUrl = "https://api.assemblyai.com/v2";

        public TranscriptionService(
            IServiceScopeFactory scopeFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TranscriptionService> logger)
        {
            _scopeFactory = scopeFactory;
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["AssemblyAI:ApiKey"] ?? string.Empty;
            _isMockMode = string.IsNullOrEmpty(_apiKey) || _apiKey == "placeholder";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", _apiKey);
        }

        public async Task ProcessAsync(Guid clinicalSessionId, string audioFilePath)
        {
            try
            {
                if (_isMockMode)
                {
                    _logger.LogInformation("TranscriptionService: running in mock mode for session {SessionId}", clinicalSessionId);
                    await MockProcessAsync(clinicalSessionId);
                }
                else
                {
                    _logger.LogInformation("TranscriptionService: uploading audio for session {SessionId}", clinicalSessionId);

                    // 1 — Upload audio file to AssemblyAI
                    var uploadUrl = await UploadAudioAsync(audioFilePath);
                    _logger.LogInformation("TranscriptionService: audio uploaded, URL received");

                    // 2 — Submit transcription job
                    var transcriptId = await SubmitTranscriptionAsync(uploadUrl);
                    _logger.LogInformation("TranscriptionService: job submitted, ID = {TranscriptId}", transcriptId);

                    // 3 — Poll until done
                    var segments = await PollUntilCompletedAsync(transcriptId);
                    _logger.LogInformation("TranscriptionService: transcription complete, {Count} segments", segments.Count);

                    // 4 — Save segments
                    await SaveSegmentsAsync(clinicalSessionId, segments);
                }

                await UpdateSessionStatusAsync(clinicalSessionId, TranscriptionStatus.Completed);
                _logger.LogInformation("TranscriptionService: session {SessionId} marked Completed", clinicalSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TranscriptionService: failed for session {SessionId}", clinicalSessionId);
                await UpdateSessionStatusAsync(clinicalSessionId, TranscriptionStatus.Failed);
            }
            finally
            {
                if (File.Exists(audioFilePath))
                {
                    File.Delete(audioFilePath);
                    _logger.LogInformation("TranscriptionService: audio file deleted");
                }
            }
        }

        // ── Step 1: Upload audio binary to AssemblyAI ──
        private async Task<string> UploadAudioAsync(string audioFilePath)
        {
            var audioBytes = await File.ReadAllBytesAsync(audioFilePath);
            var content = new ByteArrayContent(audioBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.PostAsync($"{BaseUrl}/upload", content);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"AssemblyAI upload failed ({response.StatusCode}): {body}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("upload_url").GetString()
                ?? throw new InvalidOperationException("AssemblyAI did not return an upload URL.");
        }

        // ── Step 2: Submit transcription job ──
        // NOTE: We use multichannel=true instead of speaker_labels (AI diarization).
        // The doctor's browser records a stereo file:
        //   Channel 1 (Left)  = Doctor  (local microphone)
        //   Channel 2 (Right) = Patient (WebRTC remote track)
        // AssemblyAI reads the physical channel index and returns it per utterance,
        // giving us 100% deterministic speaker attribution with zero labelling errors.
        // Speaker diarization (speaker_labels) forces AssemblyAI to *guess* who is who
        // from a mono mix, which is unreliable — especially with Arabic audio on
        // the free tier where diarization may silently fall back to a single segment.
        private async Task<string> SubmitTranscriptionAsync(string audioUrl)
        {
            var body = JsonSerializer.Serialize(new
            {
                audio_url = audioUrl,
                multichannel = true,
                language_code = "ar",
                punctuate = true,
                format_text = true
            });

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/transcript", content);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"AssemblyAI submit failed ({response.StatusCode}): {responseBody}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("AssemblyAI did not return a transcript ID.");
        }

        // ── Step 3: Poll for completion ──
        private async Task<List<TranscriptSegmentRaw>> PollUntilCompletedAsync(string transcriptId)
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                var response = await _httpClient.GetAsync($"{BaseUrl}/transcript/{transcriptId}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var status = root.GetProperty("status").GetString();

                _logger.LogInformation("TranscriptionService: poll status = {Status}", status);

                if (status == "completed")
                    return ParseSegments(root);

                if (status == "error")
                {
                    var errorMsg = root.TryGetProperty("error", out var err)
                        ? err.GetString()
                        : "AssemblyAI transcription failed with unknown error.";
                    throw new InvalidOperationException(errorMsg);
                }
            }
        }

        // ── Step 4: Parse utterances into segments ──
        // With multichannel=true, AssemblyAI returns a "channel" integer per utterance:
        //   channel == 1  →  Doctor  (Left channel, recorded from local mic)
        //   channel == 2  →  Patient (Right channel, recorded from WebRTC remote)
        // This is fully deterministic — no AI guesswork about who is speaking.
        private List<TranscriptSegmentRaw> ParseSegments(JsonElement root)
        {
            var segments = new List<TranscriptSegmentRaw>();

            // AssemblyAI returns multichannel utterances in "utterances"
            if (!root.TryGetProperty("utterances", out var utterances))
            {
                // Fallback: if utterances is absent (e.g. very short silence-only audio),
                // treat the full transcript as a single Doctor segment to avoid data loss.
                if (root.TryGetProperty("text", out var fullText))
                {
                    segments.Add(new TranscriptSegmentRaw
                    {
                        Speaker = SpeakerRole.Doctor,
                        Content = fullText.GetString() ?? string.Empty,
                        StartMs = 0,
                        EndMs = 0,
                        ConfidenceScore = null
                    });
                }
                return segments;
            }

            foreach (var utterance in utterances.EnumerateArray())
            {
                var text = utterance.GetProperty("text").GetString() ?? string.Empty;
                var start = utterance.GetProperty("start").GetInt64();
                var end = utterance.GetProperty("end").GetInt64();

                // Read the channel field — AssemblyAI multichannel returns this as an
                // integer (1-indexed) matching the physical audio channel in the file.
                int channelIndex = 1; // Default to Channel 1 (Doctor) as a safe fallback
                if (utterance.TryGetProperty("channel", out var chanProp))
                {
                    if (chanProp.ValueKind == JsonValueKind.Number)
                        channelIndex = chanProp.GetInt32();
                    else if (chanProp.ValueKind == JsonValueKind.String
                             && int.TryParse(chanProp.GetString(), out var parsedChan))
                        channelIndex = parsedChan;
                }

                // Channel 1 (Left)  = Doctor
                // Channel 2 (Right) = Patient
                var role = channelIndex == 1 ? SpeakerRole.Doctor : SpeakerRole.Patient;

                segments.Add(new TranscriptSegmentRaw
                {
                    Speaker = role,
                    Content = text,
                    StartMs = start,
                    EndMs = end,
                    ConfidenceScore = utterance.TryGetProperty("confidence", out var conf)
                        ? (decimal?)conf.GetDouble()
                        : null
                });
            }

            return segments;
        }

        // ── Mock: 5s delay then dummy segments ──
        private async Task MockProcessAsync(Guid clinicalSessionId)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            var mockSegments = new List<TranscriptSegmentRaw>
            {
                new() { Speaker = SpeakerRole.Doctor,  Content = "Good morning, how have you been feeling this week?",              StartMs = 0,     EndMs = 4000  },
                new() { Speaker = SpeakerRole.Patient, Content = "A bit better than last week, but still anxious at night.",        StartMs = 4500,  EndMs = 9000  },
                new() { Speaker = SpeakerRole.Doctor,  Content = "Can you tell me more about what happens at night?",               StartMs = 9500,  EndMs = 13000 },
                new() { Speaker = SpeakerRole.Patient, Content = "I keep waking up with racing thoughts, hard to fall back asleep.", StartMs = 13500, EndMs = 18000 },
            };

            await SaveSegmentsAsync(clinicalSessionId, mockSegments);
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
            public string Content { get; set; } = string.Empty;
            public long StartMs { get; set; }
            public long EndMs { get; set; }
            public decimal? ConfidenceScore { get; set; }
        }
    }
}