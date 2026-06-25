namespace GhaithAI.API.GaithAI.Application.DTOs.Report
{
    /// <summary>
    /// Aggregated DTO carrying all data the QuestPDF renderer needs.
    /// Built by SessionReportPdfService from multiple entity sources.
    /// </summary>
    public sealed class PdfReportDataDto
    {
        // ── Report Identity ──────────────────────────────────────────────────
        public Guid ReportId { get; set; }
        public Guid SessionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public int VersionNumber { get; set; }

        // ── Doctor (from DoctorsProfile) ─────────────────────────────────────
        public string DoctorFullName { get; set; } = string.Empty;
        public string DoctorSpecialization { get; set; } = string.Empty;
        public string DoctorType { get; set; } = string.Empty;

        // ── Patient (from ApplicationUser + UserAssessment) ─────────────────
        public string PatientFullName { get; set; } = string.Empty;
        public string PatientGender { get; set; } = string.Empty;
        public int? PatientAge { get; set; }
        public string? PatientConcerns { get; set; }
        public string? PatientSleepQuality { get; set; }
        public string? PatientStressLevel { get; set; }
        public bool PatientHasTherapyHistory { get; set; }
        public bool PatientTakesMedication { get; set; }

        // ── Session (from ClinicalSession) ───────────────────────────────────
        public DateTime SessionDate { get; set; }
        public int? SessionDurationMinutes { get; set; }
        public string SessionType { get; set; } = string.Empty;

        // ── SOAP (from SessionReport entity — reflects doctor edits) ─────────
        public string? SoapSubjective { get; set; }
        public string? SoapObjective { get; set; }
        public string? SoapAssessment { get; set; }
        public string? SoapPlan { get; set; }
        public string? PharmacologicalNote { get; set; }

        // ── Chief Complaint (from ReportJson) ────────────────────────────────
        public string? ChiefComplaintPrimary { get; set; }
        public string? ChiefComplaintDuration { get; set; }
        public string? ChiefComplaintEpisodeType { get; set; }
        public string? ChiefComplaintSecondary { get; set; }

        // ── HPI (from ReportJson) ─────────────────────────────────────────────
        public string? HpiNarrative { get; set; }
        public string? HpiFunctionalImpact { get; set; }
        public string? HpiPastHistory { get; set; }
        public string? HpiCurrentMedications { get; set; }

        // ── MSE (from ReportJson) ─────────────────────────────────────────────
        public string? MseAppearance { get; set; }
        public string? MseSpeechAndMood { get; set; }
        public string? MseThoughtProcess { get; set; }
        public string? MsePerception { get; set; }
        public string? MseInsightAndJudgement { get; set; }

        // ── Clinical Formulation (from ReportJson) ────────────────────────────
        public string? FormulationNarrative { get; set; }
        public List<string>? DifferentialConsiderations { get; set; }

        // ── Risk (from SessionReport entity) ─────────────────────────────────
        public string RiskTier { get; set; } = "LOW";
        public bool SiPresent { get; set; }
        public string? SuicidalIdeationDetails { get; set; }
        public string? RiskNarrative { get; set; }

        // ── Approval ──────────────────────────────────────────────────────────
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
    }
}
