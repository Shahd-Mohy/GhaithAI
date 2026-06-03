global using GhaithAI.API.GaithAI.Application.Services.Class;
global using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
global using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;
global using GhaithAI.API.Repositories.Class;
global using GhaithAI.API.Repositories.Interfaces;
global using GhaithAI.API.Services.Class;
global using GhaithAI.API.Services.Interfaces;
global using System.Reflection;

namespace GhaithAI.API.GaithAI.Application.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Mood & Chat & Risk Repositories
            services.AddScoped<IMoodRepository, MoodRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IRiskRepository, RiskRepository>();
            services.AddScoped<ISelfHelpRepository, SelfHelpRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpClient<ILangflowService, LangflowService>();

            // Services Registration
            services.AddScoped<IMoodService, MoodService>();
            services.AddScoped<ISelfHelpAdminService, SelfHelpAdminService>();
            services.AddScoped<ISelfHelpUserService, SelfHelpUserService>();

            return services;
        }

        public static IServiceCollection AddMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}