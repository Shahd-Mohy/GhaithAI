using GhaithAI.API.Constants;
using Microsoft.AspNetCore.Identity;

namespace GhaithAI.GaithAI.Infrastructure.Seeders
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager)
        {
            var email = "admin@ghaithai.com";

            var admin =
                await userManager.FindByEmailAsync(email);

            if (admin != null)
                return;

            admin = new ApplicationUser
            {
                FullName = "System Admin",
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsActive = true,
                PreferredLanguage = "en",
                CountryCode = "EG"
            };

            var result =
                await userManager.CreateAsync(
                    admin,
                    "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    admin,
                    Roles.Admin);
            }
        }
    }
}
