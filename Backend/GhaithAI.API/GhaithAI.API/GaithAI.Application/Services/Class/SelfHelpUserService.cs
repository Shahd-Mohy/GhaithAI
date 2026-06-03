namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    public class SelfHelpUserService : ISelfHelpUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SelfHelpUserService> _logger;

        public SelfHelpUserService(IUnitOfWork unitOfWork, IMapper mapper , ILogger<SelfHelpUserService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger=logger ?? throw new ArgumentNullException(nameof(logger)) ;
        }
        public async Task<IEnumerable<UserSelfHelpResponseDto>> GetActiveContentAsync(string? type, string? difficulty)
        {
            try
            {
                if(_unitOfWork.SelfHelp == null)
                {
                    _logger.LogError("SelfHelp repository is not available.");
                    return Enumerable.Empty<UserSelfHelpResponseDto>();
                }

                var contents = await _unitOfWork.SelfHelp.GetAllAsync();

                if (contents == null)
                    return Enumerable.Empty<UserSelfHelpResponseDto>();

                var query = contents.Where(c => c != null && c.IsActive && !c.IsDeleted);
                if (!string.IsNullOrEmpty(type))
                {
                    var cleanType = type.Trim();
                    query = query.Where(c => c.Type != null && c.Type.Equals(cleanType, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(difficulty))
                {
                    var cleanDifficulty = difficulty.Trim();
                    query = query.Where(c => c.DifficultyLevel != null && c.DifficultyLevel.Equals(cleanDifficulty, StringComparison.OrdinalIgnoreCase));
                }
                return _mapper.Map<IEnumerable<UserSelfHelpResponseDto>>(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching active self-help content.");
                throw new ApplicationException("An error occurred while fetching self-help content. Please try again later.", ex);
            }

           
        }

        public async Task<UserSelfHelpResponseDto> GetContentByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("GetContentByIdAsync called with an empty Guid.");
                return null!;
            }
            try
            {
                if (_unitOfWork.SelfHelp == null)
                {
                    _logger.LogError("SelfHelp repository is not available.");
                    return null!;
                }
                var content = await _unitOfWork.SelfHelp.GetByIdAsync(id);
                if (content == null || !content.IsActive || content.IsDeleted)
                {
                    return null!;
                }

                return _mapper.Map<UserSelfHelpResponseDto>(content);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while accessing the SelfHelp repository.");
                throw new ApplicationException("An error occurred while accessing the self-help repository, please try again later.", ex);
            }
        
        }
    }
}
