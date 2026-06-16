namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceRepository
{

    public interface IDoctorProfileRepository : IGenericRepository<DoctorsProfile>
    {

        Task<DoctorsProfile?> GetFullProfileByDoctorIdAsync(Guid doctorId);

        Task<DoctorsProfile?> GetFullProfileByUserIdAsync(string userId);

        IQueryable<DoctorsProfile> GetPublicDoctorsQueryable();

        IQueryable<DoctorsProfile> GetProfileForUpdateQueryable();
        Task<IList<DoctorCustomSchedule>> GetCustomSchedulesAsync(
            Guid doctorId,
            DateTime? fromDate = null,
            DateTime? toDate = null);
    }
}
