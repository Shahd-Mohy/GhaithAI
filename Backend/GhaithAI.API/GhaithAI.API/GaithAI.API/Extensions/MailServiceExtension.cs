using GhaithAI.API.GaithAI.API.Configurations;
using GhaithAI.API.Services.Class;
using GhaithAI.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GhaithAI.API.Extensions
{
    /// <summary>
    /// Extension methods for mail service dependency injection.
    /// </summary>
    public static class MailServiceExtension
    {
        /// <summary>
        /// Adds mail service to the dependency injection container.
        /// Configures MailSettings from appsettings and registers IMailService as Transient.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMailService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure MailSettings from appsettings.json
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));

            // Register IMailService as Transient (new instance per request)
            services.AddTransient<IMailService, MailService>();

            return services;
        }
    }
}
