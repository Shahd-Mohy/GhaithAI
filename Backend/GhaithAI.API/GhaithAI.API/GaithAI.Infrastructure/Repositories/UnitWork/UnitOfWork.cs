global using GhaithAI.GaithAI.Infrastructure.Repositories.Class;

namespace GhaithAI.API.Repositories.UnitWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private ISessionRepository _session;
        private IMessageRepository _message;
        private IRiskRepository _risk;
        private ISelfHelpRepository _selfHelp;
        private IBaseSpecialtyRepository _baseSpecialty;
        private IBaseLanguageRepository _baseLanguage;
        private IClinicPatientRepository _clinicPatient;
        private IDoctorProfileRepository _doctorProfile;
        private IExerciseTipsRepository _exerciseTips;
        private IBookingRepository _booking;
        private IDoctorCustomScheduleRepository _doctorCustomSchedule;
        private IDoctorDefaultScheduleRepository _doctorDefaultSchedule;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context),
                "ApplicationDbContext cannot be null. Ensure DbContext is properly registered in DI container.");
        }

        public ISessionRepository Session => _session ??= new SessionRepository(_context);

        public IMessageRepository Message => _message ??= new MessageRepository(_context);

        public IRiskRepository Risk => _risk ??= new RiskRepository(_context);

        public ISelfHelpRepository SelfHelp => _selfHelp ??= new SelfHelpRepository(_context);

        public IBaseSpecialtyRepository BaseSpecialty => _baseSpecialty ??= new BaseSpecialtyRepository(_context);
        public IBaseLanguageRepository BaseLanguage => _baseLanguage ??= new BaseLanguageRepository(_context);
        public IClinicPatientRepository ClinicPatient => _clinicPatient ??= new ClinicPatientRepository(_context);
        public IDoctorProfileRepository DoctorProfile => _doctorProfile ??= new DoctorProfileRepository(_context);

        public IExerciseTipsRepository ExerciseTips => _exerciseTips ??= new ExerciseTipsRepository(_context);
        public IBookingRepository Booking => _booking ??= new BookingRepository(_context);

        public IDoctorCustomScheduleRepository DoctorCustomSchedule => _doctorCustomSchedule ??= new DoctorCustomScheduleRepository(_context);
        public IDoctorDefaultScheduleRepository DoctorDefaultSchedule => _doctorDefaultSchedule ??= new DoctorDefaultScheduleRepository(_context);
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
