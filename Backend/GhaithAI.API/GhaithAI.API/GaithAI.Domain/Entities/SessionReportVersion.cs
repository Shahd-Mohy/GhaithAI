using GhaithAI.API.GaithAI.Domain.Common;


namespace GhaithAI.GaithAI.Domain.Entities
{
    public class SessionReportVersion : BaseEntity<Guid>
    {
        public Guid SessionReportId { get; set; }
        public virtual SessionReport SessionReport { get; set; }

        public int VersionNumber { get; set; }

        public string SnapshotJson { get; set; }

        public string? ChangeNote { get; set; }
        
        public string CreatedBy { get; set; }
    }
}
