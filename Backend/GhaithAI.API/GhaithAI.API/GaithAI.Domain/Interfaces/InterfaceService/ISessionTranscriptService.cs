using GhaithAI.GaithAI.Application.DTOs.Transcript;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface ISessionTranscriptService
    {
        Task UploadAndTriggerAsync(Guid sessionId, IFormFile audioFile);
        Task<TranscriptStatusResponseDto> GetStatusAsync(Guid sessionId);
        Task<IEnumerable<TranscriptSegmentResponseDto>> GetSegmentsAsync(Guid sessionId);
        Task UpdateSegmentAsync(Guid sessionId, Guid segmentId, UpdateSegmentDto dto);
    }
}
