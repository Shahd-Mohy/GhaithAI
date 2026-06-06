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
    }
}
