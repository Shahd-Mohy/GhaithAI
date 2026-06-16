using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.Models;
using GhaithAI.API.Repositories.Interfaces;

namespace GhaithAI.API.Repositories.Interfaces
{
    public interface IJournalRepository : IGenericRepository<JournalEntry>
    {
        Task<(List<JournalEntry> Items, int TotalCount)> GetByUserIdAsync(
            string userId, string? search, int page, int pageSize);
        Task<List<JournalEntry>> GetAllByUserIdAsync(string userId);
        Task<bool> BelongsToUserAsync(Guid id, string userId);
        Task SoftDeleteAsync(Guid id);
        Task<int> SaveChangesAsync();
    }
}
