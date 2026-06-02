using GhaithAI.API.GaithAI.Application.DTOs.SelfHelp;

namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface ISelfHelpAdminService
    {
        Task<IEnumerable<AdminSelfHelpResponseDto>> GetAllContentAsync();
        Task<AdminSelfHelpResponseDto> GetContentByIdAsync(Guid id);
        Task<AdminSelfHelpResponseDto> CreateContentAsync(AdminSelfHelpSaveDto dto, string adminId);
        Task<AdminSelfHelpResponseDto> UpdateContentAsync(Guid id, AdminSelfHelpSaveDto dto, string adminId);
        Task<bool> SoftDeleteContentAsync(Guid id, string adminId);
    }
}
