using GhaithAI.API.Repositories.Interfaces;

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

        IBookingRepository Booking {  get; }


        IDoctorCustomScheduleRepository DoctorCustomSchedule { get; }
        IDoctorDefaultScheduleRepository DoctorDefaultSchedule { get; }
        Task<int> CompleteAsync();
        int Complete();
    }
}
