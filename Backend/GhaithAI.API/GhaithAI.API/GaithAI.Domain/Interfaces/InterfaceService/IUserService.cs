using GhaithAI.API.DTOs.User;

namespace GhaithAI.API.Interfaces.InterfaceService
{
    public interface IUserService
    {
        Task<UserProfileDTO>
            GetProfileAsync(string userId);

        Task<bool>
            UpdateProfileAsync(
                string userId,
                UpdateProfileDTO dto);
    }
}