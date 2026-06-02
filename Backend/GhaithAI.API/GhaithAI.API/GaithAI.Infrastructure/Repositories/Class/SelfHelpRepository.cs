global using GhaithAI.API.Data;

namespace GhaithAI.API.Repositories.Class
{
    public class SelfHelpRepository : GenericRepository<SelfHelpContent>, ISelfHelpRepository
    {
        public SelfHelpRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
