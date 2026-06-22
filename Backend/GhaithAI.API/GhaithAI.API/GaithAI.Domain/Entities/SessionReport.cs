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

        public string RiskTier { get; set; }
        public bool SiPresent { get; set; }

        public string? SoapSubjective { get; set; }
        public string? SoapObjective { get; set; }
        public string? SoapAssessment { get; set; }
        public string? SoapPlan { get; set; }

        public string ReportJson { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        public virtual ICollection<SessionReportVersion> Versions { get; set; } = new List<SessionReportVersion>();
    }
}
