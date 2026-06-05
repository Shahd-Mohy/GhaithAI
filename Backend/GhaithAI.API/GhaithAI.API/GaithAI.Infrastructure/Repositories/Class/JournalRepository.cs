using GhaithAI.API.Data;
using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;
using GhaithAI.API.Models;
using GhaithAI.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Repositories.Class
{
    public class JournalRepository : GenericRepository<JournalEntry>, IJournalRepository
    {
        public JournalRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(List<JournalEntry> Items, int TotalCount)> GetByUserIdAsync(
            string userId, string? search, int page, int pageSize)
        {
            var query = _context.JournalEntries
                .Where(j => j.UserId == userId && !j.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(j =>
                    (j.Title != null && j.Title.ToLower().Contains(term)) ||
                    j.Content.ToLower().Contains(term));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<List<JournalEntry>> GetAllByUserIdAsync(string userId)
            => await _context.JournalEntries
                .Where(j => j.UserId == userId && !j.IsDeleted)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

        public async Task<bool> BelongsToUserAsync(Guid id, string userId)
            => await _context.JournalEntries
                .AnyAsync(j => j.Id == id && j.UserId == userId && !j.IsDeleted);

        public async Task SoftDeleteAsync(Guid id)
        {
            var entry = await _context.JournalEntries.FindAsync(id);
            if (entry != null)
            {
                entry.IsDeleted = true;
                _context.JournalEntries.Update(entry);
            }
        }

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
