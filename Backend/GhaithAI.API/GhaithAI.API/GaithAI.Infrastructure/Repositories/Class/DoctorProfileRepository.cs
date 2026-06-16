namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class DoctorProfileRepository : GenericRepository<DoctorsProfile>, IDoctorProfileRepository
    {
        public DoctorProfileRepository(ApplicationDbContext context) : base(context) { }

        public async Task<DoctorsProfile?> GetFullProfileByDoctorIdAsync(Guid doctorId)
        {
            return await FullProfileQuery()
                .FirstOrDefaultAsync(d => d.Id == doctorId);
        }

        public async Task<DoctorsProfile?> GetFullProfileByUserIdAsync(string userId)
        {
            return await FullProfileQuery()
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public IQueryable<DoctorsProfile> GetPublicDoctorsQueryable()
        {
            return FullProfileQuery()
                .Where(d => d.Clinic != null && d.Clinic.IsPublicListed);
        }


        public IQueryable<DoctorsProfile> GetProfileForUpdateQueryable()
        {
            return _dbSet
                .Include(d => d.Clinic)
                .Include(d => d.ServiceSetting)
                .Include(d => d.DefaultSchedules)
                .Include(d => d.CustomSchedules)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.BaseSpecialty)
                .Include(d => d.DoctorLanguages)
                    .ThenInclude(dl => dl.BaseLanguage);
        }

        public async Task<IList<DoctorCustomSchedule>> GetCustomSchedulesAsync(
            Guid doctorId,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.Set<DoctorCustomSchedule>()
                .AsNoTracking()
                .Where(cs => cs.DoctorId == doctorId);

            if (fromDate.HasValue)
                query = query.Where(cs => cs.CustomDate.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(cs => cs.CustomDate.Date <= toDate.Value.Date);

            return await query
                .OrderBy(cs => cs.CustomDate)
                .ThenBy(cs => cs.StartTime)
                .ToListAsync();
        }

        private IQueryable<DoctorsProfile> FullProfileQuery()
        {
            return _dbSet
                .AsNoTracking()
                .Include(d => d.Clinic)
                .Include(d => d.ServiceSetting)
                .Include(d => d.DefaultSchedules)
                .Include(d => d.CustomSchedules)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.BaseSpecialty)
                .Include(d => d.DoctorLanguages)
                    .ThenInclude(dl => dl.BaseLanguage)
                .Include(d => d.Reviews);
        }
    }
}
