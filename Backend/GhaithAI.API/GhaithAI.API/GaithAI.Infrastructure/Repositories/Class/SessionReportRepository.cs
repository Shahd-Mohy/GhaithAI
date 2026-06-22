using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.Presistance;
using GhaithAI.API.Repositories.Class;
using GhaithAI.GaithAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class SessionReportRepository : GenericRepository<SessionReport>, ISessionReportRepository
    {
        public SessionReportRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<SessionReport?> GetBySessionIdAsync(Guid sessionId)
        {
            return await _context.SessionReports
                .FirstOrDefaultAsync(r => r.SessionId == sessionId);
        }

        public async Task<SessionReport?> GetWithVersionsAsync(Guid reportId)
        {
            return await _context.SessionReports
                .Include(r => r.Versions)
                .FirstOrDefaultAsync(r => r.Id == reportId);
        }
    }
}
