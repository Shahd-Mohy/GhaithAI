using GhaithAI.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Entities;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class ReportFeedbackService : IReportFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReportFeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ReportFeedbackTagDto> AddTagAsync(
            Guid reportId, CreateFeedbackTagDto dto, string creatorId, string creatorRole)
        {
            var reportExists = await _unitOfWork.ClinicalReport.GetAllQueryableNoTracking()
                .AnyAsync(r => r.Id == reportId);

            if (!reportExists)
                throw new KeyNotFoundException("Report not found.");

            var tag = new ReportFeedbackTag
            {
                Id = Guid.NewGuid(),
                ClinicalReportId = reportId,
                TagType = dto.TagType,
                Notes = dto.Notes,
                CreatorId = creatorId,
                CreatorRole = creatorRole,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ReportFeedbackTag.AddAsync(tag);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ReportFeedbackTagDto>(tag);
        }

        public async Task<IEnumerable<ReportFeedbackTagDto>> GetByReportIdAsync(Guid reportId)
        {
            var tags = await _unitOfWork.ReportFeedbackTag.GetAllQueryableNoTracking()
                .Where(t => t.ClinicalReportId == reportId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReportFeedbackTagDto>>(tags);
        }

        public async Task<object> GetAnalyticsAsync(string? tagType, DateTime? from, DateTime? to)
        {
            var query = _unitOfWork.ReportFeedbackTag.GetAllQueryableNoTracking();

            if (!string.IsNullOrEmpty(tagType) && Enum.TryParse<Domain.Enums.FeedbackTagType>(tagType, true, out var parsedTag))
                query = query.Where(t => t.TagType == parsedTag);

            if (from.HasValue)
                query = query.Where(t => t.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.CreatedAt <= to.Value);

            var grouped = await query
                .GroupBy(t => t.TagType)
                .Select(g => new { TagType = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            return new
            {
                TotalFeedbackCount = grouped.Sum(g => g.Count),
                Breakdown = grouped
            };
        }
    }
}