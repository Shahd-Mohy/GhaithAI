using GhaithAI.GaithAI.Domain.Entities;

namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository
{
    public interface ISessionReportRepository : IGenericRepository<SessionReport>
    {
        Task<SessionReport?> GetBySessionIdAsync(Guid sessionId);
        Task<SessionReport?> GetWithVersionsAsync(Guid reportId);
    }
}
