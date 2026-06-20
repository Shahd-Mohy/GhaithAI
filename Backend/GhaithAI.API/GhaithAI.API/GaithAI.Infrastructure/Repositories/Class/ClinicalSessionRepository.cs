namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class ClinicalSessionRepository : GenericRepository<ClinicalSession>, IClinicalSessionRepository
    {
        public ClinicalSessionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ClinicalSession>> GetSessionsByDoctorAsync(
            Guid doctorId, ClinicalSessionStatus? status, DateTime? date)
        {
            var query = _dbSet
                .Include(s => s.Patient)
                .Where(s => s.DoctorId == doctorId);

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            if (date.HasValue)
                query = query.Where(s => s.StartedAt.Date == date.Value.Date);

            return await query.OrderByDescending(s => s.StartedAt).ToListAsync();
        }

        public async Task<IEnumerable<ClinicalSession>> GetSessionsByPatientAsync(string patientId)
        {
            return await _dbSet
                .Include(s => s.Doctor)
                .Where(s => s.PatientId == patientId)
                .OrderByDescending(s => s.StartedAt)
                .ToListAsync();
        }
    }
}
