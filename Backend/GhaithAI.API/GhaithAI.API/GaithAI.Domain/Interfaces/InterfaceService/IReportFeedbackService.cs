using GhaithAI.GaithAI.Application.DTOs.Report;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IReportFeedbackService
    {
        Task<ReportFeedbackTagDto> AddTagAsync(
            Guid reportId, CreateFeedbackTagDto dto, string creatorId, string creatorRole);

        Task<IEnumerable<ReportFeedbackTagDto>> GetByReportIdAsync(Guid reportId);

        Task<object> GetAnalyticsAsync(string? tagType, DateTime? from, DateTime? to);
    }
}
