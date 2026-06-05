using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Repositories.Class
{
    public class InsightRepository : IInsightRepository
    {
        private readonly ApplicationDbContext _context;

        public InsightRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MoodLog>> GetMoodLogsInRangeAsync(
            string userId, DateTime from, DateTime to)
        {
            return await _context.MoodLogs
                .Where(m => m.UserId == userId
                         && !m.IsDeleted
                         && m.LoggedAt >= from
                         && m.LoggedAt <= to)
                .OrderBy(m => m.LoggedAt)
                .ToListAsync();
        }
    }
}
