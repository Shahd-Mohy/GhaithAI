global using GhaithAI.API.GaithAI.Application.Services.Class;
global using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
global using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;
global using GhaithAI.API.Repositories.Class;
global using GhaithAI.API.Repositories.Interfaces;
global using GhaithAI.API.Services.Class;
global using GhaithAI.API.Services.Interfaces;
global using System.Reflection;
using GhaithAI.API.Services;
using GhaithAI.GaithAI.Application.Services;
using GhaithAI.GaithAI.Application.Services.Class;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using GhaithAI.GaithAI.Infrastructure.Services;
using GhaithAI.GaithAI.Application.Services;

namespace GhaithAI.API.GaithAI.Application.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // services.AddScoped<IUnitOfWork, UnitOfWork>();
            // services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IMoodRepository, MoodRepository>();
            services.AddScoped<IJournalRepository, JournalRepository>();
            services.AddScoped<IInsightRepository, InsightRepository>();
            services.AddScoped<IDoctorProfileRepository, DoctorProfileRepository>();
            //services.AddScoped<IChatRepository, ChatRepository>();
            //services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IRiskRepository, RiskRepository>();
            services.AddScoped<ISelfHelpRepository, SelfHelpRepository>();
            services.AddScoped<IBaseSpecialtyRepository, BaseSpecialtyRepository>();
            services.AddScoped<IBaseLanguageRepository, BaseLanguageRepository>();
            services.AddScoped<IClinicPatientRepository, ClinicPatientRepository>();
            services.AddScoped<IExerciseTipsRepository, ExerciseTipsRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IDoctorCustomScheduleRepository, DoctorCustomScheduleRepository>();
            services.AddScoped<IDoctorDefaultScheduleRepository, DoctorDefaultScheduleRepository>();
            services.AddScoped<IClinicalSessionRepository, ClinicalSessionRepository>();
            services.AddScoped<ISessionNoteRepository, SessionNoteRepository>();
            services.AddScoped<ISessionTranscriptRepository, SessionTranscriptRepository>();
            services.AddScoped<ISessionReportRepository, SessionReportRepository>();
            services.AddScoped<ISessionReportVersionRepository, SessionReportVersionRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            //services.AddHttpClient();
            //services.AddHttpClient<ILangflowService, LangflowService>();

            // Services Registration
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMoodService, MoodService>();
            services.AddScoped<IJournalService, JournalService>();
            services.AddScoped<IInsightService, InsightService>();
            services.AddScoped<IDoctorClinicProfileService, DoctorClinicProfileService>();
            services.AddScoped<ISelfHelpAdminService, SelfHelpAdminService>();
            services.AddScoped<ISelfHelpUserService, SelfHelpUserService>();
            services.AddScoped<IBaseSpecialtyService, BaseSpecialtyService>();
            services.AddScoped<IBaseLanguageService, BaseLanguageService>();
            services.AddScoped<IClinicPatientService, ClinicPatientService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IAdminDashboardStatsService, AdminDashboardStatsService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ILangFlowClient, LangFlowClient>();

            services.AddScoped<ITranscriptAggregationService, TranscriptAggregationService>();
            services.AddScoped<IClinicalSessionService, ClinicalSessionService>();
            services.AddScoped<ISessionNoteService, SessionNoteService>();

            // ── AssemblyAI HttpClient ──
            services.AddHttpClient<ITranscriptionService, TranscriptionService>(client =>
            {
                client.BaseAddress = new Uri("https://api.assemblyai.com/");
                client.Timeout = TimeSpan.FromMinutes(10);
            });

            services.AddScoped<ISessionTranscriptService, SessionTranscriptService>();
            services.AddScoped<ISessionReportService, SessionReportService>();
            services.AddScoped<ISessionReportPdfService, SessionReportPdfService>();

            services.AddScoped<IDoctorDashboardPatiantService, DoctorDashboardPatiantService>();
            services.AddScoped<IStripeClient, StripeClient>();
            services.AddScoped<IPaymentService, PaymentService>();

            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }

        public static IServiceCollection AddMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}