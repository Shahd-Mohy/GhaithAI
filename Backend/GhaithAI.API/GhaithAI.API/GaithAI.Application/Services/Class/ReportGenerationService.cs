using GhaithAI.API.Constants;
using GhaithAI.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Entities;
using GhaithAI.GaithAI.Domain.Enums;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class ReportGenerationService : IReportGenerationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILangFlowClient _langFlowClient;
        private readonly IReportHistoryService _historyService;
        private readonly ITranscriptAggregationService _transcriptAggregation;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public ReportGenerationService(
            IUnitOfWork unitOfWork,
            ILangFlowClient langFlowClient,
            IReportHistoryService historyService,
            ITranscriptAggregationService transcriptAggregation,
            IMapper mapper,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _langFlowClient = langFlowClient;
            _historyService = historyService;
            _transcriptAggregation = transcriptAggregation;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<ClinicalReportResponseDto> GenerateAsync(Guid clinicalSessionId, string requestedByUserId)
        {
            var session = await _unitOfWork.ClinicalSession.GetAllQueryableNoTracking()
                .Include(s => s.Notes)
                .FirstOrDefaultAsync(s => s.Id == clinicalSessionId)
                ?? throw new KeyNotFoundException("Clinical session not found.");

            if (session.Status != ClinicalSessionStatus.Completed)
                throw new InvalidOperationException("Cannot generate a report before the session has ended.");

            var existingReport = await _unitOfWork.ClinicalReport.GetAllQueryableNoTracking()
                .AnyAsync(r => r.ClinicalSessionId == clinicalSessionId);

            if (existingReport)
                throw new InvalidOperationException("A report already exists for this session.");

            // 1 — تجميع الـ transcript (chat messages أو voice segments حسب SessionType)
            var aggregatedTranscript = await _transcriptAggregation.BuildAsync(clinicalSessionId);

            // 2 — جمع السياق الإضافي
            var doctorNotes = session.Notes
                .OrderBy(n => n.CreatedAt)
                .Select(n => n.Content);

            var moodHistory = await _unitOfWork.MoodLog.GetAllQueryableNoTracking()
                .Where(m => m.UserId == session.PatientId && m.LoggedAt >= DateTime.UtcNow.AddDays(-7))
                .OrderBy(m => m.LoggedAt)
                .Select(m => new { Score = m.MoodScore, m.LoggedAt })
                .ToListAsync();

            var payload = new
            {
                transcript = aggregatedTranscript,
                doctorNotes = string.Join("\n", doctorNotes),
                moodHistory
            };

            // 3 — استدعاء LangFlow
            var flowId = _configuration["LangFlow:ClinicalReportFlowId"];
            var rawResult = await _langFlowClient.RunFlowAsync(flowId, payload);

            var aiOutput = JsonSerializer.Deserialize<AiReportOutput>(rawResult,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new Exception("Failed to parse LangFlow report output.");

            // 4 — حفظ كل حاجة في transaction واحدة
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var report = new ClinicalReport
                {
                    Id = Guid.NewGuid(),
                    ClinicalSessionId = clinicalSessionId,
                    ReportType = "Standard",
                    Status = ReportStatus.Draft,
                    AiDraftJson = rawResult,
                    AiModelVersion = _configuration["LangFlow:ModelVersion"] ?? "unknown",
                    IsAiGenerated = true
                };

                await _unitOfWork.ClinicalReport.AddAsync(report);

                var sections = BuildSections(report.Id, aiOutput);
                await _unitOfWork.ReportSection.AddRangeAsync(sections);

                await _unitOfWork.CompleteAsync();

                // الـ History إجبارية — بتتعمل في نفس العملية مش بعدين
                await _historyService.CreateInitialHistoryAsync(report.Id, rawResult);

                await transaction.CommitAsync();

                return await GetByIdAsync(report.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ClinicalReportResponseDto> GetByIdAsync(Guid reportId)
        {
            var report = await _unitOfWork.ClinicalReport.GetAllQueryableNoTracking()
                .Include(r => r.ReportSections.OrderBy(s => s.OrderIndex))
                .FirstOrDefaultAsync(r => r.Id == reportId)
                ?? throw new KeyNotFoundException("Report not found.");

            return _mapper.Map<ClinicalReportResponseDto>(report);
        }

        public async Task<ClinicalReportResponseDto?> GetApprovedByPatientSessionAsync(
            Guid clinicalSessionId, string patientId)
        {
            var report = await _unitOfWork.ClinicalReport.GetAllQueryableNoTracking()
                .Include(r => r.ReportSections.OrderBy(s => s.OrderIndex))
                .Include(r => r.ClinicalSession)
                .FirstOrDefaultAsync(r =>
                    r.ClinicalSessionId == clinicalSessionId &&
                    r.ClinicalSession.PatientId == patientId &&
                    r.Status == ReportStatus.Approved);

            return report == null ? null : _mapper.Map<ClinicalReportResponseDto>(report);
        }

        private static List<ReportSection> BuildSections(Guid reportId, AiReportOutput ai)
        {
            return new List<ReportSection>
            {
                new() { Id = Guid.NewGuid(), ClinicalReportId = reportId, SectionType = SectionType.Subjective, Title = "Subjective", AiContent = ai.Subjective, OrderIndex = 0 },
                new() { Id = Guid.NewGuid(), ClinicalReportId = reportId, SectionType = SectionType.Objective, Title = "Objective", AiContent = ai.Objective, OrderIndex = 1 },
                new() { Id = Guid.NewGuid(), ClinicalReportId = reportId, SectionType = SectionType.Assessment, Title = "Assessment", AiContent = ai.Assessment, OrderIndex = 2 },
                new() { Id = Guid.NewGuid(), ClinicalReportId = reportId, SectionType = SectionType.Plan, Title = "Plan", AiContent = ai.Plan, OrderIndex = 3 },
                new() { Id = Guid.NewGuid(), ClinicalReportId = reportId, SectionType = SectionType.RiskSummary, Title = "Risk Summary", AiContent = ai.RiskSummary, OrderIndex = 4 },
                new() { Id = Guid.NewGuid(), ClinicalReportId = reportId, SectionType = SectionType.Recommendations, Title = "Recommendations", AiContent = ai.Recommendations, OrderIndex = 5 },
            };
        }

        // شكل الـ JSON المتوقع من LangFlow flow
        private class AiReportOutput
        {
            public string Subjective { get; set; }
            public string Objective { get; set; }
            public string Assessment { get; set; }
            public string Plan { get; set; }
            public string RiskSummary { get; set; }
            public string Recommendations { get; set; }
        }
    }
}