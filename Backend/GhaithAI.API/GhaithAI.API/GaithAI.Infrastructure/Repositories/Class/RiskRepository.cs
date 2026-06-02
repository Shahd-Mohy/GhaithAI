using GhaithAI.API.Data;
using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;
using GhaithAI.API.Models;
using GhaithAI.API.Presistance;
using GhaithAI.API.Repositories.Interfaces;

namespace GhaithAI.API.Repositories.Class
{
    public class RiskRepository : GenericRepository<RiskEvent>, IRiskRepository
    {

        public RiskRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
