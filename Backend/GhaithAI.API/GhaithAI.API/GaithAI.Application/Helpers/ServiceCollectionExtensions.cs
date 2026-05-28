using GhaithAI.API.GaithAI.Application.Services.Class;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService;
using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;

using GhaithAI.API.Repositories.UnitWork;

namespace GhaithAI.API.GaithAI.Application.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpClient<ILangflowService, LangflowService>();
            return services;
        }

        public static IServiceCollection AddMapping(this IServiceCollection services)
        {

            return services;
        }
    }
}
