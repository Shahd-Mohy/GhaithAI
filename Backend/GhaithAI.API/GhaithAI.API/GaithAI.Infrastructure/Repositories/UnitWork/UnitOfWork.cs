
namespace GhaithAI.API.Repositories.UnitWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IChatRepository _chat;
        private IMessageRepository _message;
        private IRiskRepository _risk;
        private ISelfHelpRepository _selfHelp;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context),
                "ApplicationDbContext cannot be null. Ensure DbContext is properly registered in DI container.");
        }

        public IChatRepository Chat => _chat ??= new ChatRepository(_context);

        public IMessageRepository Message => _message ??= new MessageRepository(_context);

        public IRiskRepository Risk => _risk ??= new RiskRepository(_context);

        public ISelfHelpRepository SelfHelp => _selfHelp ??= new SelfHelpRepository(_context);
        public async Task<int> CompleteAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "An error occurred while saving changes to the database. See inner exception for details.",
                    ex);
            }
        }

        public int Complete()
        {
            try
            {
                return _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "An error occurred while saving changes to the database. See inner exception for details.",
                    ex);
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }

        ~UnitOfWork()
        {
            Dispose();
        }
    }
}
