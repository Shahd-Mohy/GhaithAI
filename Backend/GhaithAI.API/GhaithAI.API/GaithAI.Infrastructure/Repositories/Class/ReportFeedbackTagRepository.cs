namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class ReportFeedbackTagRepository : GenericRepository<ReportFeedbackTag>, IReportFeedbackTagRepository
    {
        public ReportFeedbackTagRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
