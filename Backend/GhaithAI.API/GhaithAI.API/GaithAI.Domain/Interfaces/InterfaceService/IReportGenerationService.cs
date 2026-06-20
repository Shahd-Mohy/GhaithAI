using GhaithAI.GaithAI.Application.DTOs.Report;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IReportGenerationService
    {
        Task<ClinicalReportResponseDto> GenerateAsync(Guid clinicalSessionId, string requestedByUserId);

        Task<ClinicalReportResponseDto> GetByIdAsync(Guid reportId);

        Task<ClinicalReportResponseDto?> GetApprovedByPatientSessionAsync(Guid clinicalSessionId, string patientId);
    }
}
