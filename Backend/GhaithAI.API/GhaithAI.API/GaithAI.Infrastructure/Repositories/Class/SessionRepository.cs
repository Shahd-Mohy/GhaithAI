

using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Repositories.Class
{
    public class SessionRepository : GenericRepository<ChatSession>, ISessionRepository
    {
        public SessionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        /// <inheritdoc />
        public async Task<string> GetLast30MessagesFormattedAsync(Guid sessionId)
        {
            var rawMessages = await _context.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(30)
                .ToListAsync();

            var chronologicalMessages = rawMessages.AsEnumerable().Reverse();

            var historyBuilder = new StringBuilder();
            foreach (var msg in chronologicalMessages)
            {
                historyBuilder.AppendLine($"{msg.SenderType}: {msg.Content}");
            }

            return historyBuilder.ToString();
        }


        /// <inheritdoc/>
        public async Task<ChatSession?> GetActiveSessionAsync(string userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.Status == "active")
                .OrderByDescending(s => s.CreatedAt) // safety: get latest if duplicates exist
                .FirstOrDefaultAsync();
        }


        /// <inheritdoc/>
        public async Task<(IEnumerable<ChatSession> Items, int TotalCount)> GetUserSessionsAsync(
            string userId, int page, int pageSize)
        {
            var baseQuery = _dbSet
                .AsNoTracking()
                .Where(s => s.UserId == userId);

            // Two separate DB round-trips: one for count, one for data.
            var totalCount = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }


        /// <inheritdoc/>
        public async Task<ChatSession?> GetSessionWithMessagesAsync(Guid sessionId, string userId)
        {
            // Ownership check embedded in WHERE to prevent IDOR.
            return await _dbSet
                .AsNoTracking()
                .Include(s => s.ChatMessages.OrderBy(m => m.CreatedAt))
                .Where(s => s.Id == sessionId && s.UserId == userId)
                .FirstOrDefaultAsync();
        }


        /// <inheritdoc/>
        public async Task<bool> BelongsToUserAsync(Guid sessionId, string userId)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(s => s.Id == sessionId && s.UserId == userId);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateTitleAsync(Guid sessionId, string userId, string newTitle)
        {
            // Perform a single-server-side update without loading the entity into memory.
            var affected = await _dbSet
                .Where(s => s.Id == sessionId && s.UserId == userId)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(s => s.Title, _ => newTitle)
                    .SetProperty(s => s.UpdatedAt, _ => DateTime.UtcNow));

            return affected > 0;
        }
    }
}
