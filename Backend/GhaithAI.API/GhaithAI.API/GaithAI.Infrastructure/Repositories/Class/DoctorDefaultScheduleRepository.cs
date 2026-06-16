namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class DoctorDefaultScheduleRepository
    : GenericRepository<DoctorDefaultSchedule>, IDoctorDefaultScheduleRepository
    {
        public DoctorDefaultScheduleRepository(ApplicationDbContext context) : base(context) { }
    }
}
