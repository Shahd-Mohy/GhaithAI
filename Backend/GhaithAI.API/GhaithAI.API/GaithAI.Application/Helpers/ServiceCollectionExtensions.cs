global using GhaithAI.API.GaithAI.Application.Services.Class;
global using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
global using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;
global using GhaithAI.API.Repositories.Class;
global using GhaithAI.API.Repositories.Interfaces;
global using GhaithAI.API.Services.Class;
global using GhaithAI.API.Services.Interfaces;
global using System.Reflection;
using GhaithAI.GaithAI.Application.Services.Class;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.API.GaithAI.Application.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // services.AddScoped<IUnitOfWork, UnitOfWork>();
            // services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Mood
            services.AddScoped<IMoodRepository, MoodRepository>();
            // Journal
            services.AddScoped<IJournalRepository, JournalRepository>();
            services.AddScoped<IInsightRepository, InsightRepository>();
            //services.AddScoped<IChatRepository, ChatRepository>();
            //services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IRiskRepository, RiskRepository>();
            services.AddScoped<ISelfHelpRepository, SelfHelpRepository>();
            services.AddScoped<IBaseSpecialtyRepository, BaseSpecialtyRepository>();
            services.AddScoped<IBaseLanguageRepository, BaseLanguageRepository>();
            services.AddScoped<IClinicPatientRepository , ClinicPatientRepository>();
            services.AddScoped<IExerciseTipsRepository, ExerciseTipsRepository>();
            
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            //services.AddHttpClient();
            //services.AddHttpClient<ILangflowService, LangflowService>();

            // Services Registration
            services.AddScoped<IChatService, ChatService>();
            // Mood
            services.AddScoped<IMoodService, MoodService>();
            // Journal 
            services.AddScoped<IJournalService, JournalService>();
            //home..insghts
            services.AddScoped<IInsightService, InsightService>();
            services.AddScoped<ISelfHelpAdminService, SelfHelpAdminService>();
            services.AddScoped<ISelfHelpUserService, SelfHelpUserService>();
            services.AddScoped<IBaseSpecialtyService, BaseSpecialtyService>();
            services.AddScoped<IBaseLanguageService, BaseLanguageService>();
            services.AddScoped<IClinicPatientService, ClinicPatientService>();
            services.AddScoped<IAdminService, AdminService>();
            return services;
        }

        public static IServiceCollection AddMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}