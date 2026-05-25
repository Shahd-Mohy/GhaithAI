using GhaithAI.API.Constants;
using Microsoft.AspNetCore.Identity;

namespace GhaithAI.API.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager)
        {
            var roles = new[]
            {
                Roles.Admin,
                Roles.User,
                Roles.Moderator,
                Roles.Therapist
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager
                        .CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}