global using GhaithAI.GaithAI.Infrastructure.Repositories.Class;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.GaithAI.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

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
        private IGenericRepository<DoctorSpecialty> _doctorSpecialty;
        private IGenericRepository<DoctorLanguage> _doctorLanguage;
        private IGenericRepository<DoctorDefaultSchedule> _doctorDefaultSchedule;
        private IGenericRepository<DoctorCustomSchedule> _customSchedule;
        private IClinicalReportRepository _clinicalReport;
        private IReportSectionRepository _reportSection;
        private IClinicalReportHistoryRepository _clinicalReportHistory;
        private IReportFeedbackTagRepository _reportFeedbackTag;

        // ✅ الجديدين
        //private IGenericRepository<ClinicalSession> _clinicalSession;
        private IGenericRepository<MoodLog> _moodLog;
        private IClinicalSessionRepository _clinicalSession;
        private ISessionNoteRepository _sessionNote;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ISessionRepository Session =>
            _session ??= new SessionRepository(_context);

        public IMessageRepository Message =>
            _message ??= new MessageRepository(_context);

        public IRiskRepository Risk =>
            _risk ??= new RiskRepository(_context);

        public ISelfHelpRepository SelfHelp =>
            _selfHelp ??= new SelfHelpRepository(_context);

        public IBaseSpecialtyRepository BaseSpecialty =>
            _baseSpecialty ??= new BaseSpecialtyRepository(_context);

        public IBaseLanguageRepository BaseLanguage =>
            _baseLanguage ??= new BaseLanguageRepository(_context);

        public IClinicPatientRepository ClinicPatient =>
            _clinicPatient ??= new ClinicPatientRepository(_context);

        public IDoctorProfileRepository DoctorProfile =>
            _doctorProfile ??= new DoctorProfileRepository(_context);

        public IExerciseTipsRepository ExerciseTips =>
            _exerciseTips ??= new ExerciseTipsRepository(_context);

        public IBookingRepository Booking =>
            _booking ??= new BookingRepository(_context);

        public IGenericRepository<DoctorSpecialty> DoctorSpecialty =>
            _doctorSpecialty ??= new GenericRepository<DoctorSpecialty>(_context);

        public IGenericRepository<DoctorLanguage> DoctorLanguage =>
            _doctorLanguage ??= new GenericRepository<DoctorLanguage>(_context);

        public IGenericRepository<DoctorDefaultSchedule> DoctorDefaultSchedule =>
            _doctorDefaultSchedule ??= new GenericRepository<DoctorDefaultSchedule>(_context);

        public IGenericRepository<DoctorCustomSchedule> CustomSchedule =>
            _customSchedule ??= new GenericRepository<DoctorCustomSchedule>(_context);


        public IClinicalReportRepository ClinicalReport =>
            _clinicalReport ??= new ClinicalReportRepository(_context);

        public IReportSectionRepository ReportSection =>
            _reportSection ??= new ReportSectionRepository(_context);

        public IClinicalReportHistoryRepository ClinicalReportHistory =>
            _clinicalReportHistory ??= new ClinicalReportHistoryRepository(_context);

        public IReportFeedbackTagRepository ReportFeedbackTag =>
            _reportFeedbackTag ??= new ReportFeedbackTagRepository(_context);

        // ✅ الجديدين
        //public IGenericRepository<ClinicalSession> ClinicalSession =>
        //    _clinicalSession ??= new GenericRepository<ClinicalSession>(_context);

        public IGenericRepository<MoodLog> MoodLog =>
            _moodLog ??= new GenericRepository<MoodLog>(_context);

        // ✅ Transaction
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
        public IClinicalSessionRepository ClinicalSession =>
            _clinicalSession ??= new ClinicalSessionRepository(_context);

        public ISessionNoteRepository SessionNote =>
            _sessionNote ??= new SessionNoteRepository(_context);

        public async Task<int> CompleteAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "An error occurred while saving changes to the database.",
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
                    "An error occurred while saving changes to the database.",
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