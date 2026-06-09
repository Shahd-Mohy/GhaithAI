global using GhaithAI.GaithAI.Domain.Interfaces.InterfaceRepository;

namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class BaseSpecialtyRepository : GenericRepository<BaseSpecialty>, IBaseSpecialtyRepository
    {
        public BaseSpecialtyRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
