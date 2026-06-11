
namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
