using GhaithAI.GaithAI.Domain.Entities;

namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository
{
    public interface ISessionReportVersionRepository : IGenericRepository<SessionReportVersion>
    {
        Task<IEnumerable<SessionReportVersion>> GetAllVersionsAsync(Guid reportId);
        Task<SessionReportVersion?> GetVersionAsync(Guid reportId, int versionNumber);

        /// <summary>Returns the version with the highest VersionNumber for the given report.</summary>
        Task<SessionReportVersion?> GetLatestVersionAsync(Guid reportId);
    }
}

