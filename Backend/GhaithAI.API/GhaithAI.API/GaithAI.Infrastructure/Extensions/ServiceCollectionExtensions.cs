using FluentValidation.AspNetCore;

namespace GhaithAI.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProjectServices(
            this IServiceCollection services)
        {
            services.AddControllers();

            services.AddEndpointsApiExplorer();

            //services.AddSwaggerGen();

            services.AddFluentValidationAutoValidation();

            return services;
        }
    }
}