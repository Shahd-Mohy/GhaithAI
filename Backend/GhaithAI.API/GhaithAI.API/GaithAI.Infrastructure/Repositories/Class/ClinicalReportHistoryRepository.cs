namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class ClinicalReportHistoryRepository : GenericRepository<ClinicalReportHistory>, IClinicalReportHistoryRepository
    {
        public ClinicalReportHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
