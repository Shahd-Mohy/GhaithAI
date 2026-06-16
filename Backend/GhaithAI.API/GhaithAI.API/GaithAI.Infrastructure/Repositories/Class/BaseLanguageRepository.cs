
namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class BaseLanguageRepository : GenericRepository<BaseLanguage>, IBaseLanguageRepository
    {
        public BaseLanguageRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
