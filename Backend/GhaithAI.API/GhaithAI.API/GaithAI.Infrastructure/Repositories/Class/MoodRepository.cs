using GhaithAI.API.Data;
using GhaithAI.API.Models;
using GhaithAI.API.Presistance;
using GhaithAI.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Repositories.Class
{
    public class MoodRepository : IMoodRepository
    {
        private readonly ApplicationDbContext _context;

        public MoodRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(MoodLog moodLog)
        {
            await _context.MoodLogs.AddAsync(moodLog);
        }

        public async Task<MoodLog?> GetByIdAsync(Guid moodLogId)
        {
            return await _context.MoodLogs
                .Where(m => m.MoodLogId == moodLogId && !m.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<(List<MoodLog> Items, int TotalCount)> GetByUserIdAsync(
            string userId,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize)
        {
            var query = _context.MoodLogs
                .Where(m => m.UserId == userId && !m.IsDeleted);

            if (from.HasValue)
                query = query.Where(m => m.LoggedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(m => m.LoggedAt <= to.Value);

            var total = await query.CountAsync();

            // for angular
            var items = await query
                .OrderByDescending(m => m.LoggedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<List<MoodLog>> GetCalendarAsync(string userId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            var logs = await _context.MoodLogs
                .Where(m => m.UserId == userId
                         && !m.IsDeleted
                         && m.LoggedAt >= startDate
                         && m.LoggedAt <= endDate)
                .OrderBy(m => m.LoggedAt)
                .ToListAsync();


            var calendarEntries = logs
                .GroupBy(m => m.LoggedAt.Date)       // Group all logs on the same day
                .Select(g => g.OrderByDescending(m => m.LoggedAt).First()) // Take the last one
                .OrderBy(m => m.LoggedAt.Date)
                .ToList();

            return calendarEntries;
        }

        public async Task<bool> BelongsToUserAsync(Guid moodLogId, string userId)
        {
            return await _context.MoodLogs
                .AnyAsync(m => m.MoodLogId == moodLogId && m.UserId == userId && !m.IsDeleted);
        }

        public void Update(MoodLog moodLog)
        {
            _context.MoodLogs.Update(moodLog);
        }

        public async Task SoftDeleteAsync(Guid moodLogId)
        {
            var log = await _context.MoodLogs.FindAsync(moodLogId);
            if (log != null)
            {
                log.IsDeleted = true;
                _context.MoodLogs.Update(log);
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<List<MoodLog>> GetAllByUserIdAsync(string userId, DateTime from, DateTime to)
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