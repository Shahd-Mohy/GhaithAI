namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class ReportSectionRepository : GenericRepository<ReportSection>, IReportSectionRepository
    {
        public ReportSectionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
