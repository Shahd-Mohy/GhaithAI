using GhaithAI.GaithAI.Application.DTOs.Specialty;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.Extensions.Logging;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class BaseSpecialtyService : IBaseSpecialtyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BaseSpecialtyService> _logger;
        public BaseSpecialtyService(IUnitOfWork unitOfWork , ILogger<BaseSpecialtyService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork), "UnitOfWork cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        }
        public async Task<IEnumerable<SpecialtyListDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all specialties.");
            var specialties = await _unitOfWork.BaseSpecialty.GetAllAsync();

            return specialties.Select(s => new SpecialtyListDto
            {
                Id = s.Id,
                SpecialtyName = s.SpecialtyName
            });
        }

        public async Task<SpecialtyDetailsDto> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Attempted to fetch specialty with an empty Guid.");
                throw new ArgumentException("Invalid specialty ID.");
            }

            _logger.LogInformation("Fetching specialty with ID: {SpecialtyId}", id);
            var specialty = await _unitOfWork.BaseSpecialty.GetByIdAsync(id);

            if (specialty == null)
            {
                _logger.LogWarning("Specialty with ID: {SpecialtyId} was not found.", id);
                throw new KeyNotFoundException($"Specialty with ID {id} was not found.");
            }

            return new SpecialtyDetailsDto
            {
                Id = specialty.Id,
                SpecialtyName = specialty.SpecialtyName
            };
        }

        public async Task<SpecialtyDetailsDto> CreateAsync(CreateSpecialtyDto dto)
        {
            var trimmedName = dto.SpecialtyName?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                _logger.LogWarning("Validation failed: Specialty name cannot be empty.");
                throw new ArgumentException("Specialty name cannot be empty or whitespace.");
            }

            var isDuplicate = await _unitOfWork.BaseSpecialty.GetAllQueryableNoTracking().AnyAsync(s =>
                s.SpecialtyName.ToLower() == trimmedName.ToLower());

            if (isDuplicate)
            {
                _logger.LogWarning("Creation failed: Specialty '{SpecialtyName}' already exists.", trimmedName);
                throw new InvalidOperationException($"A specialty with the name '{trimmedName}' already exists.");
            }

            var specialty = new BaseSpecialty
            {
                Id = Guid.NewGuid(),
                SpecialtyName = trimmedName
            };

            await _unitOfWork.BaseSpecialty.AddAsync(specialty);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Successfully created specialty '{SpecialtyName}' with ID: {SpecialtyId}", trimmedName, specialty.Id);
            return new SpecialtyDetailsDto
            {
                Id = specialty.Id,
                SpecialtyName = specialty.SpecialtyName
            };
        }
        public async Task<bool> UpdateAsync(UpdateSpecialtyDto dto)
        {
            if (dto.Id == Guid.Empty)
            {
                _logger.LogWarning("Update failed: Invalid specialty ID.");
                throw new ArgumentException("Invalid specialty ID.");
            }

            var trimmedName = dto.SpecialtyName?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                _logger.LogWarning("Update failed: Specialty name cannot be empty.");
                throw new ArgumentException("Specialty name cannot be empty.");
            }

            var specialty = await _unitOfWork.BaseSpecialty.GetByIdAsync(dto.Id);
            if (specialty == null)
            {
                _logger.LogWarning("Update failed: Specialty with ID {SpecialtyId} does not exist.", dto.Id);
                throw new KeyNotFoundException($"Specialty with ID {dto.Id} does not exist.");
            }

            var isDuplicate = await _unitOfWork.BaseSpecialty.GetAllQueryableNoTracking()
                .AnyAsync(s => s.SpecialtyName.ToLower() == trimmedName.ToLower() && s.Id != dto.Id);

            if (isDuplicate)
            {
                _logger.LogWarning("Update failed: Another specialty with the name '{SpecialtyName}' already exists.", trimmedName);
                throw new InvalidOperationException($"Another specialty with the name '{trimmedName}' already exists.");
            }

            specialty.SpecialtyName = trimmedName;

            _unitOfWork.BaseSpecialty.Update(specialty);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully updated specialty with ID: {SpecialtyId} to '{SpecialtyName}'", dto.Id, trimmedName);
            return true;
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Deletion failed: Invalid specialty ID.");
                throw new ArgumentException("Invalid specialty ID.");
            }

            var specialty = await _unitOfWork.BaseSpecialty.GetAllQueryableTracking()
                .Include(s => s.DoctorSpecialties)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null)
            {
                _logger.LogWarning("Deletion failed: Specialty with ID {SpecialtyId} was not found.", id);
                throw new KeyNotFoundException($"Specialty with ID {id} was not found.");
            }

            if (specialty.DoctorSpecialties != null && specialty.DoctorSpecialties.Any())
            {
                _logger.LogWarning("Deletion blocked: Specialty with ID {SpecialtyId} is linked to active doctors.", id);
                throw new InvalidOperationException("Cannot delete this specialty because it is currently linked to active doctors in the system.");
            }

            await _unitOfWork.BaseSpecialty.DeleteAsync(specialty.Id);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully deleted specialty with ID: {SpecialtyId}", id);
            return "Specialty deleted successfully.";
        }
    }
}
