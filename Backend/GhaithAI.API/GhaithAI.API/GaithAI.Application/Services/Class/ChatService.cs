using GhaithAI.API.Constants;
using GhaithAI.API.DTOs.Chat;
using GhaithAI.API.GaithAI.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Services.Class
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILangflowService _langflowService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IUnitOfWork unitOfWork,
            ILangflowService langflowService,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _langflowService = langflowService;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<SessionDTO> StartSessionAsync(string userId, StartSessionDTO dto)
        {
            // Phase 1: User Existence & Eligibility Validation
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");

            if (!user.IsActive)
                throw new UserAccountSuspendedException();

            if (!user.AcceptedAiChat)
                throw new AiConsentRequiredException();

            // Phase 2: Single Active Session Constraint 
            // Query with tracking to automatically capture state changes for persistence
            var activeSession = await _unitOfWork.Session
                .GetAllQueryableTracking()
                .Where(s => s.UserId == userId && s.Status == SessionStatus.Active)
                .FirstOrDefaultAsync();

            if (activeSession is not null)
            {
                // Update state directly in memory
                activeSession.Status = SessionStatus.Ended;
                activeSession.EndedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Auto-closed previous active session {SessionId} for user {UserId}",
                    activeSession.Id, userId);
            }

            // Phase 3: New Session Creation & Privacy Policy 
            var sessionTitle = string.IsNullOrWhiteSpace(dto.Title)
                ? $"New Session {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                : dto.Title;

            var newSession = new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Status = SessionStatus.Active,
                RiskLevel = RiskLevels.Low,
                MemoryEnabled = dto.MemoryEnabled && user.MemoryEnabled,
                Title = sessionTitle,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Session.AddAsync(newSession);

            // Phase 4: Atomic Transaction & Response Mapping
            // Single call commits both the previous session closure and new session creation
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Session {SessionId} started for user {UserId}",
                newSession.Id, userId);

            return _mapper.Map<SessionDTO>(newSession);
        }


        /// <inheritdoc/>
        public async Task<SessionDTO> EndSessionAsync(string userId, Guid sessionId)
        {
            // Phase 1: Single-Step Fetch & Ownership Check
            var session = await _unitOfWork.Session
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
                ?? throw new KeyNotFoundException("Session not found or access denied.");

            // Phase 2: State Transition
            if (session.Status == SessionStatus.Ended)
                throw new InvalidOperationException("Session is already ended.");

            session.Status = SessionStatus.Ended;
            session.EndedAt = DateTime.UtcNow;

            // Phase 3: Implicit Persistence & Logging
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Session {SessionId} successfully closed via explicit user request for user {UserId}",
                sessionId, userId);

            return _mapper.Map<SessionDTO>(session);
        }


        /// <inheritdoc/>
        public async Task<PaginatedSessionsDTO> GetUserSessionsAsync(
            string userId, int page = 1, int pageSize = 20)
        {
            // Phase 1: Input Defense
            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(page, 1);

            // Phase 2: Data Fetch
            var (items, totalCount) = await _unitOfWork.Session
                .GetUserSessionsAsync(userId, page, pageSize);

            // Phase 3: Mapping
            return new PaginatedSessionsDTO
            {
                Items = _mapper.Map<IEnumerable<SessionDTO>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<ChatHistoryDTO?> GetSessionHistoryAsync(string userId, Guid sessionId)
        {
            // Phase 1: Secure Fetch
            var session = await _unitOfWork.Session
                .GetSessionWithMessagesAsync(sessionId, userId);

            // Phase 2: Null Propagating
            if (session is null)
                return null;

            // Phase 3: Structural Schema Mapping
            return new ChatHistoryDTO
            {
                Session = _mapper.Map<SessionDTO>(session),
                Messages = _mapper.Map<IEnumerable<ChatMessageDTO>>(session.ChatMessages)
            };
        }

        /// <inheritdoc/>
        public async Task<SendMessageResponseDTO> SendMessageAsync(
            string userId, UserChatRequestDto dto)
        {
            // 1. Validation & Security 
            if (!Guid.TryParse(dto.SessionId, out var sessionId))
                throw new ArgumentException("Invalid SessionId format.");

            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new ArgumentException("Message cannot be empty.");

            // 2. Secure Ownership & Active Status Check
            var session = await _unitOfWork.Session
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
                ?? throw new KeyNotFoundException("Session not found or access denied.");

            if (session.Status != SessionStatus.Active)
            {
                _logger.LogError("This session has ended. Please start a new session.??????");
                throw new InvalidOperationException(
                    "This session has ended. Please start a new session.");
            }

            _logger.LogInformation(
                "Processing message for session {SessionId} by user {UserId}",
                sessionId, userId);

            // 3. Delegate to LangflowService for AI Processing
            var langflowResult = await _langflowService
                .ProcessUserMessageAsync(sessionId, dto.Message);

            // 4. Strategic In-Memory Mutation & Atomic Persistence 
            if (langflowResult.IsRiskDetected && langflowResult.RiskDetails is not null)
            {
                session.RiskLevel = langflowResult.RiskDetails.RiskType.ToLower();

                _logger.LogWarning(
                    "Risk detected in session {SessionId}: {RiskType} - updating RiskLevel",
                    sessionId, langflowResult.RiskDetails.RiskType);

                await _unitOfWork.CompleteAsync();
            }

            // 5. Zero-Query In-Memory Response Construction
            var nowUtc = DateTime.UtcNow;

            var userMessageDto = new ChatMessageDTO
            {
                Id = Guid.NewGuid(),
                SenderType = "User",
                Content = dto.Message,
                SentAt = nowUtc
            };

            var aiMessageDto = new ChatMessageDTO
            {
                Id = Guid.NewGuid(),
                SenderType = "AI",
                Content = langflowResult.AiResponse,
                SentAt = nowUtc
            };

            _logger.LogInformation(
                "Message exchange completed for session {SessionId}",
                sessionId);

            return new SendMessageResponseDTO
            {
                UserMessage = userMessageDto,
                AiMessage = aiMessageDto,
                IsRiskDetected = langflowResult.IsRiskDetected,
                RiskDetails = langflowResult.RiskDetails
            };
        }

        /// <inheritdoc/>
        public async Task<SessionDTO> UpdateSessionTitleAsync(string userId, Guid sessionId, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Title cannot be empty.");

            var normalized = newTitle.Trim();

            var updated = await _unitOfWork.Session.UpdateTitleAsync(sessionId, userId, normalized);

            if (!updated)
                throw new KeyNotFoundException("Session not found or access denied.");

            _logger.LogInformation(
                "Session {SessionId} title updated by user {UserId}",
                sessionId, userId);

            var session = await _unitOfWork.Session
                .GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
                ?? throw new KeyNotFoundException("Session not found after update.");

            return _mapper.Map<SessionDTO>(session);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteSessionAsync(string userId, Guid sessionId)
        {
            // Ownership check first
            if (!await _unitOfWork.Session.BelongsToUserAsync(sessionId, userId))
                throw new KeyNotFoundException("Session not found.");

            var session = await _unitOfWork.Session
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
                ?? throw new KeyNotFoundException("Session not found.");

            // For soft delete entities (ISoftDelete), use DeleteAsync
            await _unitOfWork.Session.DeleteAsync(sessionId);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Session {SessionId} deleted for user {UserId}",
                sessionId, userId);

            return true;
        }
    }
}
