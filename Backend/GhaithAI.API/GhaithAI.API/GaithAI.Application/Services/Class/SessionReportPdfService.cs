using GhaithAI.API.GaithAI.Application.DTOs.Report;
using GhaithAI.API.Presistance;
using GhaithAI.API.Repositories.UnitWork;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Text.Json;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    /// <summary>
    /// Loads all data required for the clinical session report PDF and delegates
    /// rendering to <see cref="SessionReportPdfDocument"/>.
    /// </summary>
    public sealed class SessionReportPdfService : ISessionReportPdfService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SessionReportPdfService> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SessionReportPdfService(
            IUnitOfWork unitOfWork,
            ApplicationDbContext db,
            ILogger<SessionReportPdfService> logger)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GeneratePdfAsync(Guid reportId)
        {
            _logger.LogInformation("Generating PDF for report {ReportId}", reportId);

            // ── 1. Load SessionReport with all navigation properties ──────────
            var report = await _db.SessionReports
                .Include(r => r.Clinician)
                .Include(r => r.Patient)
                .Include(r => r.ClinicalSession)
                .FirstOrDefaultAsync(r => r.Id == reportId)
                ?? throw new KeyNotFoundException($"Session report {reportId} not found.");

            // ── 2. Load UserAssessment for the patient ────────────────────────
            var assessment = await _db.UserAssessments
                .FirstOrDefaultAsync(a => a.UserId == report.PatientId);

            // ── 3. Load latest version number ─────────────────────────────────
            var latestVersion = await _unitOfWork.SessionReportVersion
                .GetLatestVersionAsync(reportId);

            // ── 4. Deserialize ReportJson to extract HPI + MSE ────────────────
            ReportJsonFullDto? fullJson = null;
            if (!string.IsNullOrWhiteSpace(report.ReportJson))
            {
                try
                {
                    fullJson = JsonSerializer.Deserialize<ReportJsonFullDto>(
                        report.ReportJson, _jsonOpts);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to deserialize ReportJson for report {ReportId}. " +
                        "PDF will render without HPI/MSE sections.", reportId);
                }
            }

            // ── 5. Aggregate all data into PdfReportDataDto ───────────────────
            var data = new PdfReportDataDto
            {
                // Identity
                ReportId = report.Id,
                SessionId = report.SessionId,
                Status = report.Status.ToString(),
                GeneratedAt = report.CreatedAt,
                VersionNumber = latestVersion?.VersionNumber ?? 1,

                // Doctor
                DoctorFullName = report.Clinician?.FullName ?? "N/A",
                DoctorSpecialization = report.Clinician?.Specialization ?? string.Empty,
                DoctorType = report.Clinician?.DoctorType.ToString() ?? string.Empty,

                // Patient (ApplicationUser)
                PatientFullName = report.Patient?.FullName ?? "N/A",
                PatientGender = report.Patient?.Gender.ToString() ?? string.Empty,

                // Patient (UserAssessment)
                PatientAge = assessment?.Age,
                PatientConcerns = assessment?.Concerns,
                PatientSleepQuality = assessment?.SleepQuality,
                PatientStressLevel = assessment?.StressLevel,
                PatientHasTherapyHistory = assessment?.HasTherapyHistory ?? false,
                PatientTakesMedication = assessment?.TakesMedication ?? false,

                // Session
                SessionDate = report.ClinicalSession?.StartedAt ?? report.CreatedAt,
                SessionDurationMinutes = report.ClinicalSession?.DurationMinutes,
                SessionType = report.ClinicalSession?.SessionType.ToString() ?? string.Empty,

                // SOAP — from entity fields (reflects doctor edits)
                SoapSubjective = report.SoapSubjective,
                SoapObjective = report.SoapObjective,
                SoapAssessment = report.SoapAssessment,
                SoapPlan = report.SoapPlan,
                PharmacologicalNote = fullJson?.Soap?.PharmacologicalNote,

                // Chief Complaint — from ReportJson
                ChiefComplaintPrimary = report.ChiefComplaintPrimary,
                ChiefComplaintDuration = report.ChiefComplaintDuration,
                ChiefComplaintEpisodeType = fullJson?.ChiefComplaint?.EpisodeType,
                ChiefComplaintSecondary = fullJson?.ChiefComplaint?.SecondaryComplaints,

                // HPI — from ReportJson
                HpiNarrative = fullJson?.HistoryOfPresentingIllness?.HpiNarrative,
                HpiFunctionalImpact = fullJson?.HistoryOfPresentingIllness?.FunctionalImpact,
                HpiPastHistory = fullJson?.HistoryOfPresentingIllness?.PastPsychiatricAndMedicalHistory,
                HpiCurrentMedications = fullJson?.HistoryOfPresentingIllness?.CurrentMedicationsAndSubstances,

                // MSE — from ReportJson
                MseAppearance = fullJson?.MentalStateExam?.AppearanceAndBehavior,
                MseSpeechAndMood = fullJson?.MentalStateExam?.SpeechAndMoodAffect,
                MseThoughtProcess = fullJson?.MentalStateExam?.ThoughtProcessAndContent,
                MsePerception = fullJson?.MentalStateExam?.PerceptionAndCognition,
                MseInsightAndJudgement = fullJson?.MentalStateExam?.InsightAndJudgement,

                // Clinical Formulation — from ReportJson + entity
                FormulationNarrative = fullJson?.ClinicalFormulation?.FormulationNarrative,
                DifferentialConsiderations = !string.IsNullOrWhiteSpace(report.DifferentialConsiderations)
                    ? JsonSerializer.Deserialize<List<string>>(
                        report.DifferentialConsiderations, _jsonOpts)
                    : null,

                // Risk — from entity fields
                RiskTier = report.RiskTier ?? "LOW",
                SiPresent = report.SiPresent,
                SuicidalIdeationDetails = report.SuicidalIdeationDetails,
                RiskNarrative = report.RiskNarrative,

                // Approval
                ApprovedAt = report.ApprovedAt,
                ApprovedBy = report.ApprovedBy,
            };

            // ── 6. Render and return PDF bytes ────────────────────────────────
            var document = new SessionReportPdfDocument(data);
            var bytes = document.GeneratePdf();

            _logger.LogInformation(
                "PDF generated for report {ReportId} — {Bytes} bytes", reportId, bytes.Length);

            return bytes;
        }
    }
}
