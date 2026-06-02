using GhaithAI.API.Models;

namespace GhaithAI.API.Repositories.Interfaces
{
    public interface IMoodRepository
    {
        Task AddAsync(MoodLog moodLog);

        Task<MoodLog?> GetByIdAsync(Guid moodLogId);

        Task<(List<MoodLog> Items, int TotalCount)> GetByUserIdAsync(
            string userId,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize);

        Task<List<MoodLog>> GetCalendarAsync(string userId, int year, int month);

        Task<bool> BelongsToUserAsync(Guid moodLogId, string userId);

        void Update(MoodLog moodLog);

        Task SoftDeleteAsync(Guid moodLogId);

        Task<int> SaveChangesAsync();
        Task<List<MoodLog>> GetAllByUserIdAsync(string userId, DateTime from, DateTime to);
    }
}
