global using AutoMapper;
global using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService;
global using GhaithAI.API.Repositories.UnitWork;
using GhaithAI.GaithAI.Application.DTOs.SelfHelp;

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

        public async Task<(IEnumerable<AdminSelfHelpGetAllDto> Items, int TotalCount)> GetAllContentAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (_unitOfWork.SelfHelp == null)
                {
                    _logger.LogError("Admin Service: SelfHelp Repository is null.");
                    return (Enumerable.Empty<AdminSelfHelpGetAllDto>(), 0);
                }

                var (contents, totalCount) = await _unitOfWork.SelfHelp.GetPagedAsync(pageNumber, pageSize);

                if (contents == null)
                    return (Enumerable.Empty<AdminSelfHelpGetAllDto>(), 0);

                var mappedItems = _mapper.Map<IEnumerable<AdminSelfHelpGetAllDto>>(contents);

                return (mappedItems, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Service: Error occurred while fetching all content with repository pagination.");
                throw new ApplicationException("A system error occurred while fetching data to the control panel.", ex);
            }
        }

        public async Task<AdminSelfHelpDetailsDTO> GetContentByIdAsync(Guid id)
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

                var content = await _unitOfWork.SelfHelp.GetAllQueryableNoTracking()
                    .Include(c=>c.ExerciseTips)
                    .FirstOrDefaultAsync(c => c.Id == id);


                if (content == null) return null!;

                return _mapper.Map<AdminSelfHelpDetailsDTO>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Service: Error fetching content with ID: {ContentId}", id);
                throw new ApplicationException($"An error occurred while attempting to retrieve details of the record with the ID{id}.", ex);
            }
        }

        public async Task<AdminSelfHelpGetAllDto> CreateContentAsync(AdminSelfHelpSaveDto dto, string adminId)
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
                contentEntity.UpdatedBy = adminId; 
                contentEntity.IsActive = dto.IsActive;

                if (contentEntity.ExerciseTips != null && contentEntity.ExerciseTips.Any())
                {
                    foreach (var tip in contentEntity.ExerciseTips)
                    {
                        tip.Id = Guid.NewGuid();
                        tip.CreatedAt = DateTime.UtcNow;
                        tip.UpdatedBy = adminId;
                    }
                }

                await _unitOfWork.SelfHelp.AddAsync(contentEntity);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Admin {AdminId} successfully created SelfHelpContent with ID: {ContentId} alongside {TipCount} tips.",
                    adminId, contentEntity.Id, contentEntity.ExerciseTips?.Count ?? 0);

                return _mapper.Map<AdminSelfHelpGetAllDto>(contentEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Service: Failed to create content by Admin: {AdminId}", adminId);
                throw new ApplicationException("The system failed to save the new content, please review the input.", ex);
            }
        }

        public async Task<bool> UpdateContentAsync(AdminSelfHelpUpdateDto dto, string adminId)
        {
            try
            {
                var content = await _unitOfWork.SelfHelp.GetAllQueryableTracking()
                    .Include(c => c.ExerciseTips)
                    .FirstOrDefaultAsync(c => c.Id == dto.Id);

                if (content == null)
                    throw new KeyNotFoundException("Self-help content record not found.");

                content.Title = dto.Title;
                content.Type = dto.Type;
                content.Description = dto.Description;
                content.ContentUrl = dto.ContentUrl;
                content.DurationMinutes = dto.DurationMinutes;
                content.DifficultyLevel = dto.DifficultyLevel;
                content.IsActive = dto.IsActive;
                content.UpdatedAt = DateTime.UtcNow;
                content.UpdatedBy = adminId;

                if (dto.ExerciseTips != null && content.ExerciseTips != null)
                {
                    foreach (var incomingTip in dto.ExerciseTips)
                    {
                        if (incomingTip.Id.HasValue && incomingTip.Id != Guid.Empty)
                        {
                            var existingTip = content.ExerciseTips.FirstOrDefault(t => t.Id == incomingTip.Id.Value);
                            if (existingTip != null)
                            {
                                existingTip.Text = incomingTip.Text;
                                existingTip.UpdatedAt = DateTime.UtcNow;
                                existingTip.UpdatedBy = adminId;
                            }
                        }
                    }
                }

                return await _unitOfWork.CompleteAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Service: Error updating content with ID: {ContentId}", dto.Id);
                throw new ApplicationException("System failed to update the content record.", ex);
            }
        }
        public async Task<bool> DeleteContentAsync(Guid id, string adminId)
        {
            try
            {
                var content = await _unitOfWork.SelfHelp.GetByIdAsync(id);
                if (content == null) throw new KeyNotFoundException("Content record not found.");

                content.IsDeleted = true;
                content.DeletedAt = DateTime.UtcNow;
                content.UpdatedBy = adminId;
                content.IsActive = false; 

                _unitOfWork.SelfHelp.Update(content);
                return await _unitOfWork.CompleteAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Service: Error while soft deleting content ID: {ContentId}", id);
                throw new ApplicationException("System failed to delete the content record.", ex);
            }
        }
 
    }
}
