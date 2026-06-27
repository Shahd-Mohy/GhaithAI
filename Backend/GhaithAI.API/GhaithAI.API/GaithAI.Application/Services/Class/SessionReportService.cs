using GhaithAI.API.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Entities;
using GhaithAI.GaithAI.Domain.Enums;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using System.Text;
using System.Text.Json;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    /// <summary>
    /// Orchestrates the full session report lifecycle:
    ///   Generate → Retrieve → Edit (SOAP) → Approve → Lock.
    ///
    /// Enforced business rules:
    ///   - Only Draft reports can be Approved.
    ///   - Locked reports cannot be edited or re-locked.
    ///   - Every successful edit creates a new <c>SessionReportVersion</c>.
    /// </summary>
    public sealed class SessionReportService : ISessionReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReportLangflowService _langflowService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SessionReportService> _logger;

        public SessionReportService(
            IUnitOfWork unitOfWork,
            IReportLangflowService langflowService,
            INotificationService notificationService,
            ILogger<SessionReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _langflowService = langflowService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<SessionReportResponseDto> GenerateReportAsync(
            Guid clinicalSessionId,
            Guid doctorId,
            string patientId)
        {
            _logger.LogInformation(
                "Generating report for ClinicalSession {SessionId}", clinicalSessionId);

            //  1. Fetch ordered transcript segments 
            var segments = (await _unitOfWork.SessionTranscript
                .GetSegmentsBySessionAsync(clinicalSessionId)).ToList();

            if (segments.Count == 0)
                throw new InvalidOperationException(
                    $"No transcript segments found for session {clinicalSessionId}. " +
                    "Cannot generate a report without a transcript.");

            // 2. Format as labelled plain-text 
            var transcriptText = FormatTranscript(segments);

            // 3. Call Langflow (HTTP delegated to IReportLangflowService) 
            var langflowResult = await _langflowService
                .AnalyzeTranscriptAsync(transcriptText, clinicalSessionId);

            // 4. Persist 
            var report = await PersistAsync(
                clinicalSessionId, doctorId, patientId, langflowResult);

            _logger.LogInformation(
                "Report {ReportId} persisted for session {SessionId}",
                report.Id, clinicalSessionId);

            await _notificationService.SendAsync(
                patientId,
                GhaithAI.GaithAI.Domain.Enums.NotificationType.SessionReportReady,
                "Session Report Ready",
                "Your session report is ready and available for review.",
                report.Id);

            return ToResponseDto(report, versionNumber: 1, generatedAt: report.CreatedAt);
        }

        //  Retrieval 

        /// <inheritdoc/>
        public async Task<SessionReportResponseDto> GetReportAsync(Guid reportId)
        {
            var report = await _unitOfWork.SessionReport.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException(
                    $"Session report {reportId} not found.");

            var latestVersion = await _unitOfWork.SessionReportVersion
                .GetLatestVersionAsync(reportId);

            return ToResponseDto(
                report,
                versionNumber: latestVersion?.VersionNumber ?? 1,
                generatedAt: report.CreatedAt);
        }

        /// <inheritdoc/>
        public async Task<SessionReportResponseDto> GetReportBySessionAsync(Guid sessionId)
        {
            var report = await _unitOfWork.SessionReport.GetBySessionIdAsync(sessionId)
                ?? throw new KeyNotFoundException(
                    $"No session report found for session {sessionId}.");

            var latestVersion = await _unitOfWork.SessionReportVersion
                .GetLatestVersionAsync(report.Id);

            return ToResponseDto(
                report,
                versionNumber: latestVersion?.VersionNumber ?? 1,
                generatedAt: report.CreatedAt);
        }

        // Mutation 

        /// <inheritdoc/>
        public async Task<SessionReportResponseDto> UpdateReportAsync(
            Guid reportId,
            Guid doctorId,
            UpdateReportDto dto)
        {
            var report = await _unitOfWork.SessionReport.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException(
                    $"Session report {reportId} not found.");

            if (report.Status == SessionReportStatus.Locked)
                throw new InvalidOperationException(
                    $"Report {reportId} is Locked and cannot be edited.");

            // Apply non-null SOAP fields 
            if (dto.SoapSubjective is not null) report.SoapSubjective = dto.SoapSubjective;
            if (dto.SoapObjective  is not null) report.SoapObjective  = dto.SoapObjective;
            if (dto.SoapAssessment is not null) report.SoapAssessment = dto.SoapAssessment;
            if (dto.SoapPlan       is not null) report.SoapPlan       = dto.SoapPlan;

            // Determine next version number 
            var latestVersion = await _unitOfWork.SessionReportVersion
                .GetLatestVersionAsync(reportId);
            var nextVersionNumber = (latestVersion?.VersionNumber ?? 0) + 1;

            // Build snapshot JSON from current state 
            var snapshotJson = JsonSerializer.Serialize(ToResponseDto(
                report, nextVersionNumber, report.CreatedAt));

            await using var tx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.SessionReport.Update(report);

                var version = new SessionReportVersion
                {
                    Id              = Guid.NewGuid(),
                    SessionReportId = report.Id,
                    VersionNumber   = nextVersionNumber,
                    SnapshotJson    = snapshotJson,
                    ChangeNote      = dto.ChangeNote ?? $"Doctor edit — version {nextVersionNumber}",
                    CreatedBy       = doctorId.ToString()
                };

                await _unitOfWork.SessionReportVersion.AddAsync(version);
                await _unitOfWork.CompleteAsync();
                await tx.CommitAsync();

                _logger.LogInformation(
                    "Report {ReportId} updated — new version {VersionNumber} committed",
                    reportId, nextVersionNumber);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            return ToResponseDto(report, nextVersionNumber, report.CreatedAt);
        }

        /// <inheritdoc/>
        public async Task<SessionReportResponseDto> ApproveReportAsync(
            Guid reportId,
            Guid doctorId)
        {
            var report = await _unitOfWork.SessionReport.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException(
                    $"Session report {reportId} not found.");

            if (report.Status != SessionReportStatus.Draft)
                throw new InvalidOperationException(
                    $"Report {reportId} cannot be approved — current status is '{report.Status}'. " +
                    "Only Draft reports can be approved.");

            report.Status     = SessionReportStatus.Approved;
            report.ApprovedAt = DateTime.UtcNow;
            report.ApprovedBy = doctorId.ToString();

            _unitOfWork.SessionReport.Update(report);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Report {ReportId} approved by doctor {DoctorId}", reportId, doctorId);

            var latestVersion = await _unitOfWork.SessionReportVersion
                .GetLatestVersionAsync(reportId);

            return ToResponseDto(
                report,
                versionNumber: latestVersion?.VersionNumber ?? 1,
                generatedAt: report.CreatedAt);
        }

        /// <inheritdoc/>
        public async Task<SessionReportResponseDto> LockReportAsync(
            Guid reportId,
            Guid doctorId)
        {
            var report = await _unitOfWork.SessionReport.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException(
                    $"Session report {reportId} not found.");

            if (report.Status == SessionReportStatus.Locked)
                throw new InvalidOperationException(
                    $"Report {reportId} is already Locked.");

            report.Status = SessionReportStatus.Locked;

            _unitOfWork.SessionReport.Update(report);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Report {ReportId} locked by doctor {DoctorId}", reportId, doctorId);

            var latestVersion = await _unitOfWork.SessionReportVersion
                .GetLatestVersionAsync(reportId);

            return ToResponseDto(
                report,
                versionNumber: latestVersion?.VersionNumber ?? 1,
                generatedAt: report.CreatedAt);
        }

        // Version queries

        /// <inheritdoc/>
        public async Task<IEnumerable<SessionReportVersionDto>> GetVersionsAsync(Guid reportId)
        {
            var versions = await _unitOfWork.SessionReportVersion
                .GetAllVersionsAsync(reportId);
            return versions.Select(ToVersionDto);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// If <paramref name="versionNumber"/> is null, returns the latest version automatically.
        /// </remarks>
        public async Task<SessionReportVersionDto> GetVersionAsync(Guid reportId, int? versionNumber = null)
        {
            SessionReportVersion? version;

            if (versionNumber.HasValue)
            {
                version = await _unitOfWork.SessionReportVersion
                    .GetVersionAsync(reportId, versionNumber.Value);

                if (version is null)
                    throw new KeyNotFoundException(
                        $"Version {versionNumber.Value} not found for report {reportId}.");
            }
            else
            {
                version = await _unitOfWork.SessionReportVersion
                    .GetLatestVersionAsync(reportId);

                if (version is null)
                    throw new KeyNotFoundException(
                        $"No versions found for report {reportId}.");
            }

            return ToVersionDto(version);
        }

        // Private helpers

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

                    //  Risk 
                    RiskTier  = ai.RiskAssessment.OverallRiskTier,
                    SiPresent = ai.RiskAssessment.SuicidalIdeationPresent,
                    SuicidalIdeationDetails = ai.RiskAssessment.SuicidalIdeationDetails,
                    RiskNarrative           = ai.RiskAssessment.RiskNarrative,

                    //  SOAP 
                    SoapSubjective = ai.Soap.Subjective,
                    SoapObjective  = ai.Soap.Objective,
                    SoapAssessment = ai.Soap.Assessment,
                    SoapPlan       = ai.Soap.Plan,

                    //  Chief Complaint 
                    ChiefComplaintPrimary  = ai.ChiefComplaint?.PrimaryConcern,
                    ChiefComplaintDuration = ai.ChiefComplaint?.Duration,

                    //  Differential Considerations (stored as JSON array) 
                    DifferentialConsiderations = ai.ClinicalFormulation?.DifferentialConsiderations is { Count: > 0 } list
                        ? JsonSerializer.Serialize(list)
                        : null,

                    //  Full AI snapshot for PDF/versioning 
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

                _logger.LogInformation(
                    "SessionReport {ReportId} and Version 1 committed to DB", report.Id);

                return report;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // Mappers

        private static SessionReportResponseDto ToResponseDto(
            SessionReport r, int versionNumber, DateTime generatedAt) => new()
        {
            ReportId    = r.Id,
            SessionId   = r.SessionId,
            Status      = r.Status.ToString(),

            // Ownership
            ClinicianId = r.ClinicianId,
            PatientId   = r.PatientId,

            // Risk
            RiskTier                = r.RiskTier,
            SiPresent               = r.SiPresent,
            SuicidalIdeationDetails = r.SuicidalIdeationDetails,
            RiskNarrative           = r.RiskNarrative,

            // SOAP
            SoapSubjective = r.SoapSubjective,
            SoapObjective  = r.SoapObjective,
            SoapAssessment = r.SoapAssessment,
            SoapPlan       = r.SoapPlan,

            // Chief Complaint
            ChiefComplaintPrimary  = r.ChiefComplaintPrimary,
            ChiefComplaintDuration = r.ChiefComplaintDuration,

            // Differential Considerations - deserialize from stored JSON array string
            DifferentialConsiderations = r.DifferentialConsiderations is not null
                ? JsonSerializer.Deserialize<List<string>>(r.DifferentialConsiderations)
                : null,

            // Approval
            ApprovedAt = r.ApprovedAt,
            ApprovedBy = r.ApprovedBy,

            // Metadata
            VersionNumber = versionNumber,
            GeneratedAt   = generatedAt
            // NOTE: ReportJson intentionally excluded — kept in DB for PDF/versioning only
        };

        private static SessionReportVersionDto ToVersionDto(SessionReportVersion v) => new()
        {
            VersionId     = v.Id,
            VersionNumber = v.VersionNumber,
            ChangeSummary = v.ChangeNote,
            SnapshotJson  = v.SnapshotJson,
            CreatedAt     = v.CreatedAt
        };
    }
}
