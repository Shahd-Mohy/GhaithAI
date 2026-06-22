using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.Presistance;
using GhaithAI.API.Repositories.Class;
using GhaithAI.GaithAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class SessionReportVersionRepository : GenericRepository<SessionReportVersion>, ISessionReportVersionRepository
    {
        public SessionReportVersionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SessionReportVersion>> GetAllVersionsAsync(Guid reportId)
        {
            return await _context.SessionReportVersions
                .Where(v => v.SessionReportId == reportId)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();
        }

        public async Task<SessionReportVersion?> GetVersionAsync(Guid reportId, int versionNumber)
        {
            return await _context.SessionReportVersions
                .FirstOrDefaultAsync(v => v.SessionReportId == reportId && v.VersionNumber == versionNumber);
        }

        /// <inheritdoc/>
        public async Task<SessionReportVersion?> GetLatestVersionAsync(Guid reportId)
        {
            return await _context.SessionReportVersions
                .Where(v => v.SessionReportId == reportId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();
        }
    }
}

