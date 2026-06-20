using GhaithAI.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Entities;
using GhaithAI.GaithAI.Domain.Enums;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class ReportHistoryService : IReportHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReportHistoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateInitialHistoryAsync(Guid clinicalReportId, string originalAiContent)
        {
            var history = new ClinicalReportHistory
            {
                Id = Guid.NewGuid(),
                ClinicalReportId = clinicalReportId,
                OriginalAiContent = originalAiContent,
                TotalChangesCount = 0,
                ModificationSeverity = ModificationSeverity.Minor,
                CapturedAt = DateTime.UtcNow
            };

            await _unitOfWork.ClinicalReportHistory.AddAsync(history);
            await _unitOfWork.CompleteAsync();
        }

        public async Task RecordEditAsync(Guid clinicalReportId)
        {
            var history = await _unitOfWork.ClinicalReportHistory.GetAllQueryableTracking()
                .Include(h => h.ClinicalReport)
                    .ThenInclude(r => r.ReportSections)
                .FirstOrDefaultAsync(h => h.ClinicalReportId == clinicalReportId)
                ?? throw new KeyNotFoundException("Report history not found.");

            history.TotalChangesCount++;
            history.ModificationSeverity = CalculateSeverity(history.ClinicalReport);

            await _unitOfWork.CompleteAsync();
        }

        public async Task FinalizeOnApprovalAsync(Guid clinicalReportId)
        {
            var history = await _unitOfWork.ClinicalReportHistory.GetAllQueryableTracking()
                .FirstOrDefaultAsync(h => h.ClinicalReportId == clinicalReportId)
                ?? throw new KeyNotFoundException("Report history not found.");

            var minutesSinceGeneration = (int)(DateTime.UtcNow - history.CapturedAt).TotalMinutes;
            history.DoctorEditDurationMinutes = minutesSinceGeneration;

            await _unitOfWork.CompleteAsync();
        }

        public async Task<ClinicalReportHistoryDto> GetByReportIdAsync(Guid clinicalReportId)
        {
            var history = await _unitOfWork.ClinicalReportHistory.GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(h => h.ClinicalReportId == clinicalReportId)
                ?? throw new KeyNotFoundException("Report history not found.");

            return _mapper.Map<ClinicalReportHistoryDto>(history);
        }

        // ── منطق تصنيف حجم التعديل بناءً على نسبة الأقسام المعدّلة ──
        private static ModificationSeverity CalculateSeverity(Domain.Entities.ClinicalReport report)
        {
            var totalSections = report.ReportSections.Count;
            if (totalSections == 0) return ModificationSeverity.Minor;

            var editedSections = report.ReportSections.Count(s => s.IsEdited);
            var ratio = (double)editedSections / totalSections;

            return ratio switch
            {
                <= 0.25 => ModificationSeverity.Minor,
                <= 0.6 => ModificationSeverity.Major,
                _ => ModificationSeverity.Rewrite
            };
        }
    }
}