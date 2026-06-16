using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.Models;

namespace GhaithAI.API.Repositories.Interfaces
{
    public interface ISelfHelpRepository : IGenericRepository<SelfHelpContent>
    {
        Task<SelfHelpContent?> GetContentWithTipsAsync(Guid id);
        Task<(IEnumerable<SelfHelpContent> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    }
}
