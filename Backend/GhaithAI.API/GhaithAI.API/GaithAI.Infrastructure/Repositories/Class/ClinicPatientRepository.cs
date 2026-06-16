
namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class ClinicPatientRepository : GenericRepository<ClinicPatient>, IClinicPatientRepository
    {
        public ClinicPatientRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
