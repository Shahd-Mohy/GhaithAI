namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    public class SelfHelpUserService : ISelfHelpUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SelfHelpUserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<IEnumerable<UserSelfHelpResponseDto>> GetActiveContentAsync(string? type, string? difficulty)
        {
            var contents = await _unitOfWork.SelfHelp.GetAllAsync();
            var query = contents.Where(c => c.IsActive && !c.IsDeleted);
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(c => c.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(difficulty))
            {
                query = query.Where(c => c.DifficultyLevel.Equals(difficulty, StringComparison.OrdinalIgnoreCase));
            }

            return _mapper.Map<IEnumerable<UserSelfHelpResponseDto>>(query);
        }

        public async Task<UserSelfHelpResponseDto> GetContentByIdAsync(Guid id)
        {
            var content = await _unitOfWork.SelfHelp.GetByIdAsync(id);
            if (content == null || !content.IsActive || content.IsDeleted)
                return null!;

            return _mapper.Map<UserSelfHelpResponseDto>(content);
        }
    }
}
