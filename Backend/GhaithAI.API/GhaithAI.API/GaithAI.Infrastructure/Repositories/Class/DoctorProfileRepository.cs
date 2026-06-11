namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class DoctorProfileRepository : GenericRepository<DoctorsProfile>, IDoctorProfileRepository
    {
        public DoctorProfileRepository(ApplicationDbContext context) : base(context) { }

        public async Task<DoctorsProfile?> GetFullProfileByDoctorIdAsync(Guid doctorId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(d => d.Clinic)
                .Include(d => d.ServiceSetting)
                .Include(d => d.DefaultSchedules)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.BaseSpecialty)
                .Include(d => d.DoctorLanguages)
                    .ThenInclude(dl => dl.BaseLanguage)
                .Include(d => d.Reviews)
                .FirstOrDefaultAsync(d => d.Id == doctorId);
        }

        public async Task<DoctorsProfile?> GetFullProfileByUserIdAsync(string userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(d => d.Clinic)
                .Include(d => d.ServiceSetting)
                .Include(d => d.DefaultSchedules)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.BaseSpecialty)
                .Include(d => d.DoctorLanguages)
                    .ThenInclude(dl => dl.BaseLanguage)
                .Include(d => d.Reviews)
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public IQueryable<DoctorsProfile> GetPublicDoctorsQueryable()
        {
            return _dbSet
                .AsNoTracking()
                .Where(d => d.Clinic != null && d.Clinic.IsPublicListed)
                .Include(d => d.Clinic)
                .Include(d => d.ServiceSetting)
                .Include(d => d.DefaultSchedules)
                .Include(d => d.DoctorSpecialties)
                    .ThenInclude(ds => ds.BaseSpecialty)
                .Include(d => d.DoctorLanguages)
                    .ThenInclude(dl => dl.BaseLanguage)
                .Include(d => d.Reviews)
                .AsQueryable();
        }
    }
}
