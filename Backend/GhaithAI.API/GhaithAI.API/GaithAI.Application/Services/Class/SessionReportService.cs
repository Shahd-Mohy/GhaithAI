using GhaithAI.API.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Entities;
using GhaithAI.GaithAI.Domain.Enums;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using System.Text;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    /// <summary>
    /// Orchestrates session report generation and manages persistence.
    ///
    /// Responsibilities:
    ///   1. Fetch ordered transcript segments from the database.
    ///   2. Format them as labelled plain-text for the AI pipeline.
    ///   3. Delegate the Langflow call to <see cref="IReportLangflowService"/>.
    ///   4. Persist the result as <c>SessionReport</c> + <c>SessionReportVersion</c> (v1).
    ///
    /// Has no HTTP or JSON-parsing logic — that is owned by <see cref="IReportLangflowService"/>.
    /// </summary>
    public sealed class SessionReportService : ISessionReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReportLangflowService _langflowService;
        private readonly ILogger<SessionReportService> _logger;

        public SessionReportService(
            IUnitOfWork unitOfWork,
            IReportLangflowService langflowService,
            ILogger<SessionReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _langflowService = langflowService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Generation
        // ─────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<SessionReportResponseDto> GenerateReportAsync(
            Guid clinicalSessionId,
            Guid doctorId,
            string patientId)
        {
            _logger.LogInformation(
                "Generating report for ClinicalSession {SessionId}", clinicalSessionId);

            // ── 1. Fetch ordered transcript segments ──────────────────────────
            var segments = (await _unitOfWork.SessionTranscript
                .GetSegmentsBySessionAsync(clinicalSessionId)).ToList();

            if (segments.Count == 0)
                throw new InvalidOperationException(
                    $"No transcript segments found for session {clinicalSessionId}. " +
                    "Cannot generate a report without a transcript.");

            // ── 2. Format as labelled plain-text ──────────────────────────────
            var transcriptText = FormatTranscript(segments);

            // ── 3. Call Langflow (HTTP delegated to IReportLangflowService) ───
            var langflowResult = await _langflowService
                .AnalyzeTranscriptAsync(transcriptText, clinicalSessionId);

            // ── 4. Persist ────────────────────────────────────────────────────
            var report = await PersistAsync(
                clinicalSessionId, doctorId, patientId, langflowResult);

            _logger.LogInformation(
                "Report {ReportId} persisted for session {SessionId}",
                report.Id, clinicalSessionId);

            return ToResponseDto(report, versionNumber: 1, generatedAt: report.CreatedAt);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Version queries
        // ─────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<IEnumerable<SessionReportVersionDto>> GetVersionsAsync(Guid reportId)
        {
            var versions = await _unitOfWork.SessionReportVersion
                .GetAllVersionsAsync(reportId);
            return versions.Select(ToVersionDto);
        }

        /// <inheritdoc/>
        public async Task<SessionReportVersionDto> GetVersionAsync(Guid reportId, int versionNumber)
        {
            var version = await _unitOfWork.SessionReportVersion
                .GetVersionAsync(reportId, versionNumber)
                ?? throw new KeyNotFoundException(
                    $"Version {versionNumber} not found for report {reportId}.");
            return ToVersionDto(version);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts ordered <see cref="SessionTranscript"/> segments to the labelled
        /// plain-text format the Langflow pipeline expects:
        ///   [Doctor:] …
        ///   [Patient:] …
        /// </summary>
        private static string FormatTranscript(IEnumerable<SessionTranscript> segments)
        {
            var sb = new StringBuilder();
            foreach (var s in segments)
            {
                var label = s.Speaker == SpeakerRole.Doctor ? "[Doctor:]" : "[Patient:]";
                sb.AppendLine($"{label} {s.Content}");
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Persists a new <see cref="SessionReport"/> and its first
        /// <see cref="SessionReportVersion"/> within a single DB transaction.
        /// </summary>
        private async Task<SessionReport> PersistAsync(
            Guid clinicalSessionId,
            Guid doctorId,
            string patientId,
            ReportLangflowResult result)
        {
            await using var tx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var ai = result.ParsedOutput;

                var report = new SessionReport
                {
                    Id          = Guid.NewGuid(),
                    SessionId   = clinicalSessionId,
                    ClinicianId = doctorId,
                    PatientId   = patientId,
                    Status      = SessionReportStatus.Draft,

                    RiskTier  = ai.RiskAssessment.OverallRiskTier,
                    SiPresent = ai.RiskAssessment.SuicidalIdeationPresent,

                    SoapSubjective = ai.Soap.Subjective,
                    SoapObjective  = ai.Soap.Objective,
                    SoapAssessment = ai.Soap.Assessment,
                    SoapPlan       = ai.Soap.Plan,

                    ReportJson = result.RawJson
                };

                await _unitOfWork.SessionReport.AddAsync(report);

                var version = new SessionReportVersion
                {
                    Id              = Guid.NewGuid(),
                    SessionReportId = report.Id,
                    VersionNumber   = 1,
                    SnapshotJson    = result.RawJson,
                    ChangeNote      = "Initial AI-generated report (v1)",
                    CreatedBy       = doctorId.ToString()
                };

                await _unitOfWork.SessionReportVersion.AddAsync(version);

                await _unitOfWork.CompleteAsync();
                await tx.CommitAsync();

                return report;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Mappers
        // ─────────────────────────────────────────────────────────────────────

        private static SessionReportResponseDto ToResponseDto(
            SessionReport r, int versionNumber, DateTime generatedAt) => new()
        {
            ReportId       = r.Id,
            SessionId      = r.SessionId,
            Status         = r.Status.ToString(),
            RiskTier       = r.RiskTier,
            SiPresent      = r.SiPresent,
            SoapSubjective = r.SoapSubjective,
            SoapObjective  = r.SoapObjective,
            SoapAssessment = r.SoapAssessment,
            SoapPlan       = r.SoapPlan,
            ReportJson     = r.ReportJson,
            VersionNumber  = versionNumber,
            GeneratedAt    = generatedAt
        };

        private static SessionReportVersionDto ToVersionDto(SessionReportVersion v) => new()
        {
            VersionId     = v.Id,
            VersionNumber = v.VersionNumber,
            ChangeSummary = v.ChangeNote,
            ReportJson    = v.SnapshotJson,
            CreatedAt     = v.CreatedAt
        };
    }
}
