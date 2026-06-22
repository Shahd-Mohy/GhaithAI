using GhaithAI.GaithAI.Domain.Entities;

namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository
{
    public interface ISessionReportVersionRepository : IGenericRepository<SessionReportVersion>
    {
        Task<IEnumerable<SessionReportVersion>> GetAllVersionsAsync(Guid reportId);
        Task<SessionReportVersion?> GetVersionAsync(Guid reportId, int versionNumber);
    }
}
