using GhaithAI.GaithAI.Application.DTOs.Language;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class BaseLanguageService : IBaseLanguageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BaseLanguageService> _logger;

        public BaseLanguageService(IUnitOfWork unitOfWork, ILogger<BaseLanguageService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork), "UnitOfWork cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        }

        public async Task<IEnumerable<LanguageListDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all languages for UI/Dropdown.");
            var languages = await _unitOfWork.BaseLanguage.GetAllAsync();

            return languages.Select(l => new LanguageListDto
            {
                Id = l.Id,
                LanguageName = l.LanguageName
            });
        }

        public async Task<LanguageDetailsDto> GetByIdAsync(Guid id)
        {
          
            if(id == Guid.Empty)
            {
                _logger.LogWarning("Attempted to fetch language with an empty Guid.");
                throw new ArgumentException("Invalid language ID.");
            }

            _logger.LogInformation("Fetching language with ID: {LanguageId}", id);
            var language = await _unitOfWork.BaseLanguage.GetByIdAsync(id);

            if (language == null)
            {
                _logger.LogWarning("Language with ID: {LanguageId} was not found.", id);
                throw new KeyNotFoundException($"Language with ID '{id}' was not found.");
            }

            return new LanguageDetailsDto
            {
                Id = language.Id,
                LanguageName = language.LanguageName
            };
        }

        public async Task<LanguageDetailsDto> CreateAsync(CreateLanguageDto dto)
        {
            var trimmedName = dto.LanguageName?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                _logger.LogWarning("Validation failed: Name cannot be empty.");
                throw new ArgumentException("Name cannot be empty or whitespace.");
            }

            var nameExists = await _unitOfWork.BaseLanguage.GetAllQueryableNoTracking()
                .AnyAsync(l => l.LanguageName.ToLower() == trimmedName.ToLower());

            if (nameExists)
            {
                _logger.LogWarning("Creation failed: Language Name '{LanguageName}' already exists.", trimmedName);
                throw new InvalidOperationException($"A language with the name '{trimmedName}' already exists.");
            }

            var language = new BaseLanguage
            {
                Id = Guid.NewGuid(), 
                LanguageName = trimmedName
            };

            await _unitOfWork.BaseLanguage.AddAsync(language);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully created language '{LanguageName}' with Code: {LanguageId}", trimmedName);

            return new LanguageDetailsDto
            {
                Id = language.Id,
                LanguageName = language.LanguageName
            };
        }

        public async Task<bool> UpdateAsync(UpdateLanguageDto dto)
        {
            var trimmedName = dto.LanguageName?.Trim();

            if ( string.IsNullOrWhiteSpace(trimmedName))
                throw new ArgumentException("Language ID and Name cannot be empty.");

            var language = await _unitOfWork.BaseLanguage.GetByIdAsync(dto.Id);
            if (language == null)
            {
                _logger.LogWarning("Update failed: Language with Code {LanguageId} does not exist.", dto.Id);
                throw new KeyNotFoundException($"Language with Code '{dto.Id}' does not exist.");
            }

            var isDuplicateName = await _unitOfWork.BaseLanguage.GetAllQueryableNoTracking()
                .AnyAsync(l => l.LanguageName.ToLower() == trimmedName.ToLower() && l.Id != dto.Id);

            if (isDuplicateName)
            {
                _logger.LogWarning("Update failed: Another language with the name '{LanguageName}' already exists.", trimmedName);
                throw new InvalidOperationException($"Another language with the name '{trimmedName}' already exists.");
            }

            language.LanguageName = trimmedName;

            _unitOfWork.BaseLanguage.Update(language);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully updated language Code: {LanguageId} to '{LanguageName}'", dto.Id, trimmedName);
            return true;
        }

        public async Task<string> DeleteAsync(Guid id)
        {
           if(id == Guid.Empty)
            {
                _logger.LogWarning("Deletion failed: Invalid language ID.");
                throw new ArgumentException("Invalid language ID.");
            }


            var language = await _unitOfWork.BaseLanguage.GetByIdAsync(id);

            if (language == null)
            {
                _logger.LogWarning("Deletion failed: Language with Code {LanguageId} was not found.", id);
                throw new KeyNotFoundException($"Language with Code '{id}' was not found.");
            }

            if (language.DoctorLanguages != null && language.DoctorLanguages.Any())
            {
                _logger.LogWarning("Deletion blocked: Language '{LanguageId}' is linked to active doctors.", id);
                throw new InvalidOperationException("Cannot delete this language because it is currently linked to active doctors in the system.");
            }

            await _unitOfWork.BaseLanguage.DeleteAsync(language);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully deleted language Code: {LanguageId}", id);
            return "Language deleted successfully.";
        }

     
    }
}
    