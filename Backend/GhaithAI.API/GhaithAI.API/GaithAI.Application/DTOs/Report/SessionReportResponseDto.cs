namespace GhaithAI.API.GaithAI.Application.DTOs.Report
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Public-facing response DTOs returned from ISessionReportService.
    // These are what the Controller/API consumer receives.
    //
    // NOTE: ReportJson is intentionally excluded here — it is persisted in the DB
    // for PDF generation and versioning, but is too large to include in the UI
    // response payload. The Frontend receives only the extracted, actionable fields.
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returned after a successful report generation.
    /// Contains all fields the Frontend needs to render the doctor's review UI.
    /// </summary>
    public sealed class SessionReportResponseDto
    {
        public Guid ReportId { get; set; }
        public Guid SessionId { get; set; }
        public string Status { get; set; } = string.Empty;

        // ── Risk Summary ─────────────────────────────────────────────────────
        public string RiskTier { get; set; } = string.Empty;
        public bool SiPresent { get; set; }

        /// <summary>Verbatim SI quotes and indicator details (shown in risk alert).</summary>
        public string? SuicidalIdeationDetails { get; set; }

        /// <summary>Narrative text explaining risk level (shown in risk card sidebar).</summary>
        public string? RiskNarrative { get; set; }

        // ── SOAP (editable by doctor) ─────────────────────────────────────────
        public string? SoapSubjective { get; set; }
        public string? SoapObjective { get; set; }
        public string? SoapAssessment { get; set; }
        public string? SoapPlan { get; set; }

        // ── Chief Complaint (read-only sidebar) ───────────────────────────────
        public string? ChiefComplaintPrimary { get; set; }
        public string? ChiefComplaintDuration { get; set; }

        // ── Clinical Formulation (read-only sidebar) ──────────────────────────
        /// <summary>Differential diagnosis suggestions from the AI (list of strings).</summary>
        public List<string>? DifferentialConsiderations { get; set; }

        // ── Metadata ──────────────────────────────────────────────────────────
        public int VersionNumber { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Returned when fetching the list of versions for a report.
    /// Includes SnapshotJson so the Frontend can compare or restore a version.
    /// </summary>
    public sealed class SessionReportVersionDto
    {
        public Guid VersionId { get; set; }
        public int VersionNumber { get; set; }
        public string? ChangeSummary { get; set; }
        public string SnapshotJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

