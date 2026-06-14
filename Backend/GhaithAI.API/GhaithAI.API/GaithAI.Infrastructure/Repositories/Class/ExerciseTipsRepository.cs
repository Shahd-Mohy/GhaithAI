
namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class ExerciseTipsRepository : GenericRepository<ExerciseTip>, IExerciseTipsRepository
    {
        public ExerciseTipsRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
