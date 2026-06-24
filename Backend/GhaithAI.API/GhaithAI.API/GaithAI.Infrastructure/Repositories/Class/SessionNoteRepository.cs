namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class SessionNoteRepository : GenericRepository<SessionNote>, ISessionNoteRepository
    {
        public SessionNoteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SessionNote>> GetNotesBySessionAsync(Guid clinicalSessionId)
        {
            return await _dbSet
                .Where(n => n.ClinicalSessionId == clinicalSessionId)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SessionNote>> GetNotesByDoctorAsync(Guid doctorId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(n => n.ClinicalSession.DoctorId == doctorId)
                .Include(n => n.ClinicalSession)
                    .ThenInclude(cs => cs.Patient)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
