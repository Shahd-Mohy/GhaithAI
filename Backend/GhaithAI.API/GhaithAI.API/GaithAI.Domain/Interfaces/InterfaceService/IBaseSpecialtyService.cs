using GhaithAI.GaithAI.Application.DTOs.Specialty;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IBaseSpecialtyService
    {
        Task<IEnumerable<SpecialtyListDto>> GetAllAsync();
        Task<SpecialtyDetailsDto> GetByIdAsync(Guid id);
        Task<SpecialtyDetailsDto> CreateAsync(CreateSpecialtyDto dto);
        Task<bool> UpdateAsync(UpdateSpecialtyDto dto);
        Task<string> DeleteAsync(Guid id);

    }
}
