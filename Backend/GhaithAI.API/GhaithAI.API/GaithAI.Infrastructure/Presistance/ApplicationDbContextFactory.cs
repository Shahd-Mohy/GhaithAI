//using GhaithAI.API.Data;
//using GhaithAI.API.Presistance;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.Configuration;

//namespace GhaithAI.API.Data
//{
//    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
//    {
//        public ApplicationDbContext CreateDbContext(string[] args)
//        {
//            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "GaithAI.API");

//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(basePath)
//                .AddJsonFile("appsettings.json", optional: false)
//                .Build();

//            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
//            optionsBuilder.UseSqlServer(
//                configuration.GetConnectionString("DefaultConnection"));

//            return new ApplicationDbContext(optionsBuilder.Options);
//        }
//    }
//}