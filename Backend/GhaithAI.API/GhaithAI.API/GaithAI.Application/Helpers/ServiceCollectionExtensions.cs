using GhaithAI.API.GaithAI.Application.Services.Class;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService;
using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;
using GhaithAI.API.Repositories.Class;
using GhaithAI.API.Repositories.Interfaces;
using GhaithAI.API.Repositories.UnitWork;
using GhaithAI.API.Services.Class;
using GhaithAI.API.Services.Interfaces;
using System.Reflection;

namespace GhaithAI.API.GaithAI.Application.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Mood
            services.AddScoped<IMoodRepository, MoodRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IRiskRepository, RiskRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpClient<ILangflowService, LangflowService>();

            // Mood
            services.AddScoped<IMoodService, MoodService>();

            return services;
        }

        public static IServiceCollection AddMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}