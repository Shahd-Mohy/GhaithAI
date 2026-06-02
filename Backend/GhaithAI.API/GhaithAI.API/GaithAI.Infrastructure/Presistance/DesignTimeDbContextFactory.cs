//using GhaithAI.API.Presistance;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;

//namespace GhaithAI.API.Data
//{
//    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
//    {
//        public ApplicationDbContext CreateDbContext(string[] args)
//        {
//            IConfigurationRoot configuration = new ConfigurationBuilder()
//                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "GaithAI.API"))
//                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//                .Build();

//            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
//            var connectionString = configuration.GetConnectionString("DefaultConnection");

//            builder.UseSqlServer(connectionString);

//            return new ApplicationDbContext(builder.Options);
//        }
//    }
//}
