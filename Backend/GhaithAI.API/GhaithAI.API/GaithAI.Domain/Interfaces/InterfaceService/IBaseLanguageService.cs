using GhaithAI.GaithAI.Application.DTOs.Language;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IBaseLanguageService
    {
        Task<IEnumerable<LanguageListDto>> GetAllAsync();
        Task<LanguageDetailsDto> GetByIdAsync(Guid id);
        Task<LanguageDetailsDto> CreateAsync(CreateLanguageDto dto);
        Task<bool> UpdateAsync(UpdateLanguageDto dto);
        Task<string> DeleteAsync(Guid id);
    }
}
