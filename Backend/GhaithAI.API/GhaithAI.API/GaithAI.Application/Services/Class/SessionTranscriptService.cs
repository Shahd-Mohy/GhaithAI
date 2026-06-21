using GhaithAI.GaithAI.Application.DTOs.Transcript;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class SessionTranscriptService : ISessionTranscriptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITranscriptionService _transcriptionService;
        private const string AudioTempFolder = "audio-temp";

        public SessionTranscriptService(
            IUnitOfWork unitOfWork,
            ITranscriptionService transcriptionService)
        {
            _unitOfWork = unitOfWork;
            _transcriptionService = transcriptionService;
        }

        public async Task UploadAndTriggerAsync(Guid sessionId, IFormFile audioFile)
        {
            var session = await _unitOfWork.ClinicalSession
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId)
                ?? throw new KeyNotFoundException("Session not found.");

            if (session.Status != ClinicalSessionStatus.Completed)
                throw new InvalidOperationException("Transcription can only be started for completed sessions.");

            Directory.CreateDirectory(AudioTempFolder);
            var filePath = Path.Combine(AudioTempFolder, $"session-{sessionId}.webm");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            session.TranscriptionStatus = TranscriptionStatus.Processing;
            await _unitOfWork.CompleteAsync();

            _ = Task.Run(() => _transcriptionService.ProcessAsync(sessionId, filePath));
        }

        public async Task<TranscriptStatusResponseDto> GetStatusAsync(Guid sessionId)
        {
            var session = await _unitOfWork.ClinicalSession
                .GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId)
                ?? throw new KeyNotFoundException("Session not found.");

            return new TranscriptStatusResponseDto
            {
                SessionId = sessionId,
                TranscriptionStatus = session.TranscriptionStatus?.ToString() ?? "NotStarted"
            };
        }

        public async Task<IEnumerable<TranscriptSegmentResponseDto>> GetSegmentsAsync(Guid sessionId)
        {
            var segments = await _unitOfWork.SessionTranscript
                .GetSegmentsBySessionAsync(sessionId);

            return segments.Select(s => new TranscriptSegmentResponseDto
            {
                Id = s.Id,
                ClinicalSessionId = s.ClinicalSessionId,
                Speaker = s.Speaker.ToString(),
                Content = s.Content,
                StartMs = s.StartMs,
                EndMs = s.EndMs,
                ConfidenceScore = s.ConfidenceScore,
                IsEdited = s.IsEdited,
                EditedAt = s.EditedAt
            });
        }

        public async Task UpdateSegmentAsync(Guid sessionId, Guid segmentId, UpdateSegmentDto dto)
        {
            var segment = await _unitOfWork.SessionTranscript
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(s => s.Id == segmentId && s.ClinicalSessionId == sessionId)
                ?? throw new KeyNotFoundException("Transcript segment not found.");

            segment.Content = dto.Content;
            segment.IsEdited = true;
            segment.EditedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
        }
    }
}
