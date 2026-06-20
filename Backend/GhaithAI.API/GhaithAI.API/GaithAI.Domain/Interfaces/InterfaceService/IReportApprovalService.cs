using GhaithAI.GaithAI.Application.DTOs.Report;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IReportApprovalService
    {
        Task<ClinicalReportResponseDto> UpdateSectionAsync(
            Guid reportId, Guid sectionId, UpdateReportSectionDto dto, string doctorId);

        Task<ClinicalReportResponseDto> UpdateDoctorNotesAsync(
            Guid reportId, UpdateDoctorNotesDto dto, string doctorId);

        Task<ClinicalReportResponseDto> ApproveAsync(
            Guid reportId, ApproveReportDto dto, string doctorId);

        Task<string> ExportPdfAsync(Guid reportId);
    }
}
