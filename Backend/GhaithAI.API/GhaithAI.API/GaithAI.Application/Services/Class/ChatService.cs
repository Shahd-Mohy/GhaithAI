using GhaithAI.API.Constants;
using GhaithAI.API.DTOs.Chat;
using GhaithAI.API.GaithAI.Domain.Exceptions;
using GhaithAI.API.Interfaces.InterfaceService;
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
        private readonly IUserService _userService;
        private readonly IMoodService _moodService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IUnitOfWork unitOfWork,
            ILangflowService langflowService,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IUserService userService,
            IMoodService moodService,
            ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _langflowService = langflowService;
            _mapper = mapper;
            _userManager = userManager;
            _userService = userService;
            _moodService = moodService;
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
            var activeSessions = await _unitOfWork.Session
                .GetAllQueryableTracking()
                .Where(s => s.UserId == userId && s.Status == ChatSessionStatus.Active).ToListAsync();
            //.FirstOrDefaultAsync();

            foreach (var activeSession in activeSessions)
            {
                if (activeSession is not null)
                {
                    // Update state directly in memory
                    activeSession.Status = ChatSessionStatus.Ended;
                    activeSession.EndedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Auto-closed previous active session {SessionId} for user {UserId}",
                        activeSession.Id, userId);
                }
            }

            // Phase 3: New Session Creation & Privacy Policy 
            var sessionTitle = string.IsNullOrWhiteSpace(dto.Title)
                ? $"New Session {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                : dto.Title;

            var newSession = new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Status = ChatSessionStatus.Active,
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
            if (session.Status == ChatSessionStatus.Ended)
                throw new InvalidOperationException("Session is already ended.");

            session.Status = ChatSessionStatus.Ended;
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

            if (session.Status != ChatSessionStatus.Active)
            {
                _logger.LogWarning(
                    "Session {SessionId} is no longer active. User {UserId} attempted to send a message.",
                    sessionId, userId);

                throw new InvalidOperationException(
                    "This session has ended. Please start a new session.");
            }

            _logger.LogInformation(
                "Processing message for session {SessionId} by user {UserId}",
                sessionId, userId);

            // ── 3. Fetch enriched AI context (conversation + user profile + mood)
            var conversationHistory = await _unitOfWork.Session
                .GetLast30MessagesFormattedAsync(sessionId);

            var userProfile = await _userService.GetProfileAsync(userId);
            var moodContext = await _moodService.GetLast7DaysMoodSummaryAsync(userId);

            var contextPackage = new AiContextPackageDto
            {
                ConversationHistory = conversationHistory,
                UserContext = new AiUserContextDto
                {
                    FullName = userProfile.FullName,
                    PreferredLanguage = userProfile.PreferredLanguage,
                    CountryCode = userProfile.CountryCode,
                    MemoryEnabled = userProfile.MemoryEnabled
                },
                MoodContext = moodContext
            };

            // 4. Delegate to Langflow (pure HTTP — no DB side-effects)
            var langflowResult = await _langflowService
                .ProcessUserMessageAsync(sessionId, dto.Message, contextPackage);

            // 5. Persist messages & risk (ChatService owns DB writes)
            var nowUtc = DateTime.UtcNow;

            var userMessageEntity = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                SenderType = SenderTypes.User,
                Content = dto.Message,
                CreatedAt = nowUtc
            };

            var aiMessageEntity = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                SenderType = SenderTypes.AI,
                Content = langflowResult.AiResponse,
                CreatedAt = nowUtc
            };

            await _unitOfWork.Message.AddAsync(userMessageEntity);
            await _unitOfWork.Message.AddAsync(aiMessageEntity);

            if (langflowResult.IsRiskDetected && langflowResult.RiskDetails is not null)
            {
                session.RiskLevel = langflowResult.RiskDetails.RiskType.ToLower();

                var riskEntity = _mapper.Map<RiskEvent>(langflowResult.RiskDetails);
                riskEntity.SessionId = sessionId;
                riskEntity.MessageId = userMessageEntity.Id;

                await _unitOfWork.Risk.AddAsync(riskEntity);

                _logger.LogWarning(
                    "Risk detected in session {SessionId}: {RiskType} — updating session risk level.",
                    sessionId, langflowResult.RiskDetails.RiskType);
            }

            // ── 6. Single atomic commit
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Message exchange persisted for session {SessionId}. UserMsgId={UserMsgId} AiMsgId={AiMsgId}",
                sessionId, userMessageEntity.Id, aiMessageEntity.Id);

            // ── 7. Return DTOs whose IDs match what was saved
            return new SendMessageResponseDTO
            {
                UserMessage = new ChatMessageDTO
                {
                    Id = userMessageEntity.Id,
                    SenderType = SenderTypes.User,
                    Content = userMessageEntity.Content,
                    SentAt = nowUtc
                },
                AiMessage = new ChatMessageDTO
                {
                    Id = aiMessageEntity.Id,
                    SenderType = SenderTypes.AI,
                    Content = aiMessageEntity.Content,
                    SentAt = nowUtc
                },
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
            // Lightweight ownership check without loading the entity
            var exists = await _unitOfWork.Session
                .GetAllQueryableNoTracking()
                .AnyAsync(s => s.Id == sessionId && s.UserId == userId);

            if (!exists)
                throw new KeyNotFoundException("Session not found or access denied.");

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
