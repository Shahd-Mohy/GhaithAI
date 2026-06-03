global using AutoMapper;
global using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService;
global using GhaithAI.API.Repositories.UnitWork;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    public class SelfHelpAdminService : ISelfHelpAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SelfHelpAdminService> _logger;
        public SelfHelpAdminService(IUnitOfWork unitOfWork , IMapper mapper , ILogger<SelfHelpAdminService> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger=logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<AdminSelfHelpResponseDto>> GetAllContentAsync()
        {
            try
            {
                if (_unitOfWork.SelfHelp == null)
                {
                    _logger.LogError("Admin Service: SelfHelp Repository is null.");
                    return Enumerable.Empty<AdminSelfHelpResponseDto>();
                }

                var contents = await _unitOfWork.SelfHelp.GetAllAsync();
                if (contents == null)
                    return Enumerable.Empty<AdminSelfHelpResponseDto>();

                return _mapper.Map<IEnumerable<AdminSelfHelpResponseDto>>(contents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Service: Error occurred while fetching all content.");
                throw new ApplicationException(" system error occurred while fetching data to the control panel.", ex);
            }
        }

        public async Task<AdminSelfHelpResponseDto> GetContentByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Admin Service: GetContentByIdAsync called with an empty Guid.");
                return null!;
            }
            try
            {
                if (_unitOfWork.SelfHelp == null)
                    return null!;

                var content = await _unitOfWork.SelfHelp.GetByIdAsync(id);
                if (content == null) return null!;

                return _mapper.Map<AdminSelfHelpResponseDto>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Service: Error fetching content with ID: {ContentId}", id);
                throw new ApplicationException($"An error occurred while attempting to retrieve details of the record with the ID{id}.", ex);
            }
        }

        public async Task<AdminSelfHelpResponseDto> CreateContentAsync(AdminSelfHelpSaveDto dto, string adminId)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto), "New content data cannot be empty.");
            if (string.IsNullOrWhiteSpace(adminId))
            {
                _logger.LogCritical("Security Warning: Attempted to create content without a valid Admin Identity.");
                throw new ArgumentException("The Admin ID is invalid or missing.");
            }
            try
            {
                if (_unitOfWork.SelfHelp == null)
                    throw new InvalidOperationException("SelfHelp Repository is unavailable.");
                var contentEntity = _mapper.Map<SelfHelpContent>(dto);
                contentEntity.Id = Guid.NewGuid();
                contentEntity.CreatedAt = DateTime.UtcNow;
                contentEntity.IsDeleted = false;
                contentEntity.DeletedAt = null;
                contentEntity.UpdatedAt = null;
                contentEntity.UpdatedBy = null;
                await _unitOfWork.SelfHelp.AddAsync(contentEntity);
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Admin {AdminId} successfully created SelfHelpContent with ID: {ContentId}", adminId, contentEntity.Id);
                return _mapper.Map<AdminSelfHelpResponseDto>(contentEntity);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Service: Failed to create content by Admin: {AdminId}", adminId);
                throw new ApplicationException("The system failed to save the new content, please review the input.", ex);
            }
        }

        public async Task<AdminSelfHelpResponseDto> UpdateContentAsync(Guid id, AdminSelfHelpSaveDto dto, string adminId)
        {
            if (id == Guid.Empty || dto == null || string.IsNullOrWhiteSpace(adminId))
            {
                _logger.LogWarning("Admin Service: Update called with invalid arguments. ID: {Id}, Admin: {AdminId}", id, adminId);
                return null!;
            }

            try
            {
                if (_unitOfWork.SelfHelp == null)
                    return null!;

                var contentEntity = await _unitOfWork.SelfHelp.GetByIdAsync(id);

                if (contentEntity == null || contentEntity.IsDeleted)
                {
                    _logger.LogWarning("Admin {AdminId} tried to update a non-existent or soft-deleted record with ID: {ContentId}", adminId, id);
                    return null!;
                }

                _mapper.Map(dto, contentEntity);

                contentEntity.UpdatedAt = DateTime.UtcNow;
                contentEntity.UpdatedBy = adminId.Trim();

                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Admin {AdminId} successfully updated content ID: {ContentId}", adminId, id);

                return _mapper.Map<AdminSelfHelpResponseDto>(contentEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Service: Failed to update content ID: {ContentId} by Admin: {AdminId}", id, adminId);
                throw new ApplicationException("An error occurred while saving changes to the content.", ex);
            }
        }

        public async Task<bool> SoftDeleteContentAsync(Guid id, string adminId)
        {
            if (id == Guid.Empty || string.IsNullOrWhiteSpace(adminId))
            {
                _logger.LogWarning("Admin Service: Delete called with invalid identifiers. ID: {Id}, Admin: {AdminId}", id, adminId);
                return false;
            }

            try
            {
                if (_unitOfWork.SelfHelp == null)
                    return false;

                var contentEntity = await _unitOfWork.SelfHelp.GetByIdAsync(id);

                if (contentEntity == null || contentEntity.IsDeleted)
                {
                    _logger.LogWarning("Admin {AdminId} attempted to delete an already deleted or missing record ID: {ContentId}", adminId, id);
                    return false;
                }

                contentEntity.IsDeleted = true;
                contentEntity.DeletedAt = DateTime.UtcNow;
                contentEntity.UpdatedAt = DateTime.UtcNow; 
                contentEntity.UpdatedBy = adminId.Trim();

                await _unitOfWork.CompleteAsync();
                _logger.LogWarning("Admin {AdminId} SOFT-DELETED content ID: {ContentId}", adminId, id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Service: Exception thrown during soft delete of ID: {ContentId} by Admin: {AdminId}", id, adminId);
                throw new ApplicationException("The system failed to complete the soft delete operation.", ex);
            }
        }
    }
}
