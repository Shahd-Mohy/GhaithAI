using GhaithAI.API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GhaithAI.API.Helpers
{
    public static class JWTTokenHelper
    {
        public static string GenerateToken(
            ApplicationUser user,
            IConfiguration configuration,
            IList<string>? roles = null)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id),

                new Claim(
                    ClaimTypes.Email,
                    user.Email ?? string.Empty),

                new Claim(
                    ClaimTypes.Name,
                    user.FullName ?? string.Empty)
            };

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(
                        new Claim(
                            ClaimTypes.Role,
                            role));
                }
            }

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        configuration["JWT:SecretKey"]!));

            var creds =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

            var token =
                new JwtSecurityToken(
                    issuer:
                        configuration["JWT:Issuer"],

                    audience:
                        configuration["JWT:Audience"],

                    claims: claims,

                    expires:
                        DateTime.UtcNow.AddDays(7),

                    signingCredentials: creds);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}