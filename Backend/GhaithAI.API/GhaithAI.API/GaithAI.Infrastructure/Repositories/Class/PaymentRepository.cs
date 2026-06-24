namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context) { }
    }
}
