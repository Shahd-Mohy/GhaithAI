using GhaithAI.GaithAI.Application.DTOs.Report;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IReportHistoryService
    {
        Task CreateInitialHistoryAsync(Guid clinicalReportId, string originalAiContent);

        Task RecordEditAsync(Guid clinicalReportId);

        Task FinalizeOnApprovalAsync(Guid clinicalReportId);

        Task<ClinicalReportHistoryDto> GetByReportIdAsync(Guid clinicalReportId);
    }
}
