using GhaithAI.API.GaithAI.Domain.Common;
using GhaithAI.API.Models;
using GhaithAI.GaithAI.Domain.Enums;

namespace GhaithAI.GaithAI.Domain.Entities
{
    public class SessionReport : AuditableEntity<Guid>
    {
        public Guid SessionId { get; set; }
        public virtual ClinicalSession ClinicalSession { get; set; }

        public string PatientId { get; set; }
        public virtual ApplicationUser Patient { get; set; }

        public Guid ClinicianId { get; set; }
        public virtual DoctorsProfile Clinician { get; set; }

        public SessionReportStatus Status { get; set; } = SessionReportStatus.Draft;

        // ── Risk ──────────────────────────────────────────────────────────────
        public string RiskTier { get; set; }
        public bool SiPresent { get; set; }

        /// <summary>Verbatim SI details and indicators extracted by the AI.</summary>
        public string? SuicidalIdeationDetails { get; set; }

        /// <summary>Narrative explanation of risk factors and protective factors.</summary>
        public string? RiskNarrative { get; set; }

        // ── SOAP — editable by the doctor ────────────────────────────────────
        public string? SoapSubjective { get; set; }
        public string? SoapObjective { get; set; }
        public string? SoapAssessment { get; set; }
        public string? SoapPlan { get; set; }

        // ── Chief Complaint Summary ──────────────────────────────────────────
        /// <summary>Short summary of the patient's primary concern.</summary>
        public string? ChiefComplaintPrimary { get; set; }

        /// <summary>Duration of the current episode (e.g. "two weeks").</summary>
        public string? ChiefComplaintDuration { get; set; }

        // ── Clinical Formulation ─────────────────────────────────────────────
        /// <summary>
        /// JSON-serialized List&lt;string&gt; of differential diagnosis suggestions.
        /// Stored as a JSON array string (e.g. ["Burnout", "Recurrent MDD"]).
        /// </summary>
        public string? DifferentialConsiderations { get; set; }

        // ── Full AI snapshot — used for PDF export & versioning only ─────────
        /// <summary>Full raw JSON returned by the Langflow pipeline. Not sent to Frontend.</summary>
        public string ReportJson { get; set; }

        // ── Approval ─────────────────────────────────────────────────────────
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        // ── Navigation ───────────────────────────────────────────────────────
        public virtual ICollection<SessionReportVersion> Versions { get; set; } = new List<SessionReportVersion>();
    }
}

