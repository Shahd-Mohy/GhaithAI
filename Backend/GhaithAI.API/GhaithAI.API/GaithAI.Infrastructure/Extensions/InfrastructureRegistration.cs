using GhaithAI.API.Data;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;
using GhaithAI.API.Repositories.UnitWork;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Extensions
{
    public static class InfrastructureRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is not configured. " +
                    "Ensure it exists in appsettings.json under ConnectionStrings section.");
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString,
                    sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 3)));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }
    }
}
