namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class ClinicalReportRepository : GenericRepository<ClinicalReport>, IClinicalReportRepository
    {
        public ClinicalReportRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
