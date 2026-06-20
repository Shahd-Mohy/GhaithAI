using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.GaithAI.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace GhaithAI.API.Repositories.UnitWork
{
    public interface IUnitOfWork : IDisposable
    {
        ISessionRepository Session { get; }
        IMessageRepository Message { get; }
        IRiskRepository Risk { get; }
        ISelfHelpRepository SelfHelp { get; }
        IBaseSpecialtyRepository BaseSpecialty { get; }
        IBaseLanguageRepository BaseLanguage { get; }
        IClinicPatientRepository ClinicPatient { get; }
        IDoctorProfileRepository DoctorProfile { get; }
        IExerciseTipsRepository ExerciseTips { get; }
        IBookingRepository Booking { get; }
        IGenericRepository<DoctorSpecialty> DoctorSpecialty { get; }
        IGenericRepository<DoctorLanguage> DoctorLanguage { get; }
        IGenericRepository<DoctorDefaultSchedule> DoctorDefaultSchedule { get; }
        IGenericRepository<DoctorCustomSchedule> CustomSchedule { get; }
        IClinicalReportRepository ClinicalReport { get; }
        IReportSectionRepository ReportSection { get; }
        IClinicalReportHistoryRepository ClinicalReportHistory { get; }
        IReportFeedbackTagRepository ReportFeedbackTag { get; }

        // ✅ الناقصين
        //IGenericRepository<ClinicalSession> ClinicalSession { get; }
        IGenericRepository<MoodLog> MoodLog { get; }

        // ✅ Transaction
        Task<IDbContextTransaction> BeginTransactionAsync();

        IClinicalSessionRepository ClinicalSession { get; }
        ISessionNoteRepository SessionNote { get; }
        Task<int> CompleteAsync();
        int Complete();
    }
}