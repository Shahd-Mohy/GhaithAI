

using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Repositories.Class
{
    public class SessionRepository : GenericRepository<ChatSession>, ISessionRepository
    {
        public SessionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

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
    }
}
