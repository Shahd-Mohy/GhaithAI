global using AutoMapper;
global using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService;
global using GhaithAI.API.Repositories.UnitWork;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    public class SelfHelpAdminService : ISelfHelpAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public SelfHelpAdminService(IUnitOfWork unitOfWork , IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AdminSelfHelpResponseDto>> GetAllContentAsync()
        {
            var contents = await _unitOfWork.SelfHelp.GetAllAsync();
            return _mapper.Map<IEnumerable<AdminSelfHelpResponseDto>>(contents);
        }

        public async Task<AdminSelfHelpResponseDto> GetContentByIdAsync(Guid id)
        {
            var content = await _unitOfWork.SelfHelp.GetByIdAsync(id);
            if (content == null) return null!;
            return _mapper.Map<AdminSelfHelpResponseDto>(content);
        }

        public async Task<AdminSelfHelpResponseDto> CreateContentAsync(AdminSelfHelpSaveDto dto, string adminId)
        {
            var contentEntity = _mapper.Map<SelfHelpContent>(dto);

            contentEntity.Id = Guid.NewGuid();
            contentEntity.CreatedAt = DateTime.UtcNow;
            contentEntity.IsDeleted = false;
            contentEntity.DeletedAt = null;
            contentEntity.UpdatedAt = null;
            contentEntity.UpdatedBy = null;


            await _unitOfWork.SelfHelp.AddAsync(contentEntity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<AdminSelfHelpResponseDto>(contentEntity);
        }

        public async Task<AdminSelfHelpResponseDto> UpdateContentAsync(Guid id, AdminSelfHelpSaveDto dto, string adminId)
        {
            var contentEntity = await _unitOfWork.SelfHelp.GetByIdAsync(id);
            if (contentEntity == null) return null!;

            _mapper.Map(dto, contentEntity);

            contentEntity.UpdatedAt = DateTime.UtcNow;
            contentEntity.UpdatedBy = adminId;

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<AdminSelfHelpResponseDto>(contentEntity);
        }

        public async Task<bool> SoftDeleteContentAsync(Guid id, string adminId)
        {
            var contentEntity = await _unitOfWork.SelfHelp.GetByIdAsync(id);
            if (contentEntity == null) return false;

            // «·Õ–› «·‰«⁄„ (Soft Delete)
            contentEntity.IsDeleted = true;
            contentEntity.DeletedAt = DateTime.UtcNow;
            contentEntity.UpdatedBy = adminId;

            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
