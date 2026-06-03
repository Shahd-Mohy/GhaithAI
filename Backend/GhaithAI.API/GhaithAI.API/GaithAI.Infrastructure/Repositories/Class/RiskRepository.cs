global using GhaithAI.API.Presistance;

namespace GhaithAI.API.Repositories.Class
{
    public class RiskRepository : GenericRepository<RiskEvent>, IRiskRepository
    {

        public RiskRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
