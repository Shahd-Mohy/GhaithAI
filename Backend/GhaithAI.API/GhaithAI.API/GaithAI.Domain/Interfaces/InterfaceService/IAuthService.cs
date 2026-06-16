using GhaithAI.API.DTOs.Auth;
using GhaithAI.GaithAI.Application.DTOs.Auth;

namespace GhaithAI.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(
            RegisterDTO dto);

        Task<AuthResponseDTO> LoginAsync(
            LoginDTO dto);
        Task<AuthResponseDTO> GoogleLoginAsync(
            GoogleLoginDTO dto);

        Task<AuthResponseDTO> RegisterClinicianAsync(
            RegisterClinicianDTO dto);
    }
}