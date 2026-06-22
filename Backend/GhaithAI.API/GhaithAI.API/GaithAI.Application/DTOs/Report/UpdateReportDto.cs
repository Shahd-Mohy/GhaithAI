namespace GhaithAI.API.GaithAI.Application.DTOs.Report
{
    /// <summary>
    /// Payload sent by the doctor when editing a session report.
    /// Only SOAP fields are editable — risk and AI fields are read-only.
    /// Saving a successful update always creates a new SessionReportVersion automatically.
    /// </summary>
    public sealed class UpdateReportDto
    {
        // ── SOAP (all optional — only provided fields are updated) ────────────

        /// <summary>Subjective section: patient's own words / reported symptoms.</summary>
        public string? SoapSubjective { get; set; }

        /// <summary>Objective section: clinician's observable findings.</summary>
        public string? SoapObjective { get; set; }

        /// <summary>Assessment section: clinical diagnosis and impression.</summary>
        public string? SoapAssessment { get; set; }

        /// <summary>Plan section: treatment plan and follow-up steps.</summary>
        public string? SoapPlan { get; set; }

        // ── Versioning Metadata ───────────────────────────────────────────────

        /// <summary>
        /// Optional note describing what was changed in this edit.
        /// Stored as ChangeNote on the new SessionReportVersion record.
        /// Example: "Corrected diagnosis in Assessment section."
        /// </summary>
        public string? ChangeNote { get; set; }
    }
}
