namespace GhaithAI.GaithAI.Infrastructure.Seeders
{
    public class CountrySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var hasEgypt = await context.Countries.AnyAsync(c => c.Id == "EG");

            if (!hasEgypt)
            {
                var egypt = new Country
                {
                    Id = "EG",
                    CountryName = "Egypt",
                    IsoCode = "eg",
                    CreatedAt = DateTime.UtcNow
                };

                await context.Countries.AddAsync(egypt);
                await context.SaveChangesAsync();
            }
        }
    }
}
