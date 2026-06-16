using System;

namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class DoctorCustomScheduleRepository
    : GenericRepository<DoctorCustomSchedule>, IDoctorCustomScheduleRepository
    {
        public DoctorCustomScheduleRepository(ApplicationDbContext context) : base(context) { }
    }
}
