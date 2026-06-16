using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.GaithAI.Domain.Entities;

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
        Task<int> CompleteAsync();
        int Complete();
    }
}