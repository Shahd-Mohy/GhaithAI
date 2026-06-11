using GhaithAI.API.GaithAI.Application.DTOs.SelfHelp;
using GhaithAI.GaithAI.Application.DTOs.SelfHelp;

namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface ISelfHelpAdminService
    {
        Task<IEnumerable<AdminSelfHelpGetAllDto>> GetAllContentAsync();
        Task<AdminSelfHelpDetailsDTO> GetContentByIdAsync(Guid id);
        Task<AdminSelfHelpGetAllDto> CreateContentAsync(AdminSelfHelpSaveDto dto, string adminId);
        Task<bool> UpdateContentAsync(AdminSelfHelpUpdateDto dto, string adminId);
        Task<bool> DeleteContentAsync(Guid id, string adminId);
    }
}
