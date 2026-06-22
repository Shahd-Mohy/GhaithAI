using GhaithAI.GaithAI.Domain.Enums;

namespace GhaithAI.API.GaithAI.Application.DTOs.Report
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Public-facing response DTOs returned from ISessionReportService.
    // These are what the Controller/API consumer receives.
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returned after a successful report generation.
    /// Contains all persisted fields that the frontend needs.
    /// </summary>
    public sealed class SessionReportResponseDto
    {
        public Guid ReportId { get; set; }
        public Guid SessionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RiskTier { get; set; } = string.Empty;
        public bool SiPresent { get; set; }

        public string? SoapSubjective { get; set; }
        public string? SoapObjective { get; set; }
        public string? SoapAssessment { get; set; }
        public string? SoapPlan { get; set; }

        /// <summary>Full raw JSON blob returned by the AI (stored for audit/versioning).</summary>
        public string ReportJson { get; set; } = string.Empty;

        public int VersionNumber { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Returned when fetching the list of versions for a report.
    /// </summary>
    public sealed class SessionReportVersionDto
    {
        public Guid VersionId { get; set; }
        public int VersionNumber { get; set; }
        public string? ChangeSummary { get; set; }
        public string ReportJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
