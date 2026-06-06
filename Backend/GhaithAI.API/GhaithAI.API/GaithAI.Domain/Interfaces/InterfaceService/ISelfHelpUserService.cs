using GhaithAI.GaithAI.Application.DTOs.SelfHelp;

namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface ISelfHelpUserService
    {
        Task<IEnumerable<UserSelfHelpResponseDto>> GetActiveContentAsync(string? type, string? difficulty);
        Task<UserSelfHelpDetailsResponseDto> GetContentByIdAsync(Guid id);
    }
}
