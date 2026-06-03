using GhaithAI.API.DTOs.User;
using GhaithAI.API.Models;
using GhaithAI.API.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Identity;
using GhaithAI.API.Services.Interfaces;

namespace GhaithAI.API.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser>
            _userManager;

        public UserService(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserProfileDTO>
            GetProfileAsync(string userId)
        {
            var user =
                await _userManager
                    .FindByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            return new UserProfileDTO
            {
                FullName = user.FullName,

                Email = user.Email,

                CountryCode = user.CountryCode,

                PreferredLanguage =
                    user.PreferredLanguage,

                MemoryEnabled =
                    user.MemoryEnabled,

                ProfilePicture =
                    user.ProfilePicture
            };
        }

        public async Task<bool>
            UpdateProfileAsync(
                string userId,
                UpdateProfileDTO dto)
        {
            var user =
                await _userManager
                    .FindByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.FullName =
                dto.FullName;

            user.PreferredLanguage =
                dto.PreferredLanguage;

            user.MemoryEnabled =
                dto.MemoryEnabled;

            var result =
                await _userManager
                    .UpdateAsync(user);

            return result.Succeeded;
        }
    }
}