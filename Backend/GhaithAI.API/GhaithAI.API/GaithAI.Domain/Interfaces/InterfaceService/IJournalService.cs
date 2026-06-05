using GhaithAI.API.DTOs.Journal;
using GhaithAI.API.GaithAI.Application.DTOs.Journal;

namespace GhaithAI.API.Services.Interfaces
{
    public interface IJournalService
    {
        // GET /api/journal 
        Task<(List<JournalDTO> Items, int TotalCount)> GetAllAsync(
            string userId,
            string? search,
            int page,
            int pageSize);

        // GET /api/journal/{id
        Task<JournalDTO?> GetByIdAsync(string userId, Guid journalId);

        // POST /api/journal 
        Task<JournalCreatedDTO> CreateAsync(string userId, CreateJournalDTO dto);

        // PUT /api/journal/{id}
        Task<bool> UpdateAsync(string userId, Guid journalId, UpdateJournalDTO dto);

        // DELETE /api/journal/{id}
        Task<bool> DeleteAsync(string userId, Guid journalId);
    }
}
