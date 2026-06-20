using GhaithAI.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Enums;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class ReportApprovalService : IReportApprovalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReportHistoryService _historyService;
        private readonly IPdfExportService _pdfExportService;
        private readonly IMapper _mapper;

        public ReportApprovalService(
            IUnitOfWork unitOfWork,
            IReportHistoryService historyService,
            IPdfExportService pdfExportService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _historyService = historyService;
            _pdfExportService = pdfExportService;
            _mapper = mapper;
        }

        public async Task<ClinicalReportResponseDto> UpdateSectionAsync(
            Guid reportId, Guid sectionId, UpdateReportSectionDto dto, string doctorId)
        {
            var report = await GetReportOrThrowAsync(reportId);

            EnsureDoctorOwnsReport(report, doctorId);
            EnsureNotApproved(report);

            var section = report.ReportSections.FirstOrDefault(s => s.Id == sectionId)
                ?? throw new KeyNotFoundException("Section not found.");

            section.DoctorContent = dto.DoctorContent;
            section.IsEdited = true;

            await _unitOfWork.CompleteAsync();

            // كل تعديل بيتسجل في الـ History — مش بس عند الـ approve
            await _historyService.RecordEditAsync(reportId);

            return _mapper.Map<ClinicalReportResponseDto>(report);
        }

        public async Task<ClinicalReportResponseDto> UpdateDoctorNotesAsync(
            Guid reportId, UpdateDoctorNotesDto dto, string doctorId)
        {
            var report = await GetReportOrThrowAsync(reportId);

            EnsureDoctorOwnsReport(report, doctorId);
            EnsureNotApproved(report);

            report.DoctorNotes = dto.DoctorNotes;
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ClinicalReportResponseDto>(report);
        }

        public async Task<ClinicalReportResponseDto> ApproveAsync(
            Guid reportId, ApproveReportDto dto, string doctorId)
        {
            var report = await GetReportOrThrowAsync(reportId);

            EnsureDoctorOwnsReport(report, doctorId);
            EnsureNotApproved(report);

            // بناء الـ FinalContent من الـ sections (DoctorContent لو موجود وإلا AiContent)
            report.FinalContent = BuildFinalContent(report);
            report.Status = ReportStatus.Approved;
            report.ApprovedAt = DateTime.UtcNow;
            report.ApprovedBy = doctorId;

            await _unitOfWork.CompleteAsync();

            // قفل الإحصائيات النهائية في الـ History
            await _historyService.FinalizeOnApprovalAsync(reportId);

            // TODO: NotificationService.SendAsync(patientId, "ReportReady", ...)

            return _mapper.Map<ClinicalReportResponseDto>(report);
        }

        public async Task<string> ExportPdfAsync(Guid reportId)
        {
            var report = await GetReportOrThrowAsync(reportId);

            if (report.Status != ReportStatus.Approved)
                throw new InvalidOperationException("Only approved reports can be exported.");

            if (!string.IsNullOrEmpty(report.PdfUrl))
                return report.PdfUrl;

            var pdfUrl = await _pdfExportService.GenerateAsync(report);

            report.PdfUrl = pdfUrl;
            await _unitOfWork.CompleteAsync();

            return pdfUrl;
        }

        // ── Helpers ──

        private async Task<Domain.Entities.ClinicalReport> GetReportOrThrowAsync(Guid reportId)
        {
            return await _unitOfWork.ClinicalReport.GetAllQueryableTracking()
                .Include(r => r.ReportSections)
                .Include(r => r.ClinicalSession)
                .FirstOrDefaultAsync(r => r.Id == reportId)
                ?? throw new KeyNotFoundException("Report not found.");
        }

        private static void EnsureDoctorOwnsReport(Domain.Entities.ClinicalReport report, string doctorId)
        {
            if (report.ClinicalSession.DoctorId.ToString() != doctorId)
                throw new UnauthorizedAccessException("You do not have access to this report.");
        }

        private static void EnsureNotApproved(Domain.Entities.ClinicalReport report)
        {
            if (report.Status == ReportStatus.Approved)
                throw new InvalidOperationException("This report has already been approved and cannot be modified.");
        }

        private static string BuildFinalContent(Domain.Entities.ClinicalReport report)
        {
            var ordered = report.ReportSections.OrderBy(s => s.OrderIndex);
            return string.Join("\n\n", ordered.Select(s =>
                $"## {s.Title}\n{(string.IsNullOrEmpty(s.DoctorContent) ? s.AiContent : s.DoctorContent)}"));
        }
    }
}