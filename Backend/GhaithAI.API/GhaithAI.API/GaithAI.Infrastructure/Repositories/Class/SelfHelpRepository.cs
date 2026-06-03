global using GhaithAI.API.Data;
using GhaithAI.API.Presistance;

namespace GhaithAI.API.Repositories.Class
{
    public class SelfHelpRepository : GenericRepository<SelfHelpContent>, ISelfHelpRepository
    {
        public SelfHelpRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
