using GhaithAI.GaithAI.Application.DTOs.ClinicalSession;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IClinicalSessionService
    {
        Task<Guid> StartSessionAsync(StartSessionDto dto);
        Task<SessionResponseDto> GetSessionByIdAsync(Guid sessionId);
        Task<SessionResponseDto> GetSessionByBookingIdAsync(Guid bookingId);
        Task EndSessionAsync(Guid sessionId);
        Task<IEnumerable<SessionResponseDto>> GetDoctorSessionsAsync(string doctorUserId, ClinicalSessionStatus? status, DateTime? date);
        Task<IEnumerable<SessionResponseDto>> GetPatientSessionsAsync(string patientId);
    }
}
