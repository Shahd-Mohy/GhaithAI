global using GhaithAI.API.Data;
using GhaithAI.API.Presistance;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Repositories.Class
{
    public class SelfHelpRepository : GenericRepository<SelfHelpContent>, ISelfHelpRepository
    {
        public SelfHelpRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<SelfHelpContent?> GetContentWithTipsAsync(Guid id)
        {
            return await _context.SelfHelpContents
                                 .Include(c => c.ExerciseTips) 
                                 .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<(IEnumerable<SelfHelpContent> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.SelfHelpContents
                                .AsNoTracking()
                                .Where(x => !x.IsDeleted);
            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
