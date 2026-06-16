using GhaithAI.API.DTOs.Chat;
using GhaithAI.API.Services.Interfaces;
using GhaithAI.API.GaithAI.Application.DTOs.Chat;
using GhaithAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        /// <summary>
        /// PUT /api/Chat/sessions/{sessionId}/title � Update session title.
        /// </summary>
        [HttpPut("sessions/{sessionId:guid}/title")]
        public async Task<IActionResult> UpdateSessionTitle(Guid sessionId, [FromBody] GhaithAI.API.GaithAI.Application.DTOs.Chat.UpdateSessionTitleDTO dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { Message = "Title cannot be empty." });

            _logger.LogInformation("User {UserId} requests title update for session {SessionId}", UserId, sessionId);

            var updated = await _chatService.UpdateSessionTitleAsync(UserId, sessionId, dto.Title);
            return Ok(updated);
        }

        /// <summary>Gets the authenticated user's ID from the JWT claims.</summary>
        private string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");

        // Session Endpoints 

        /// <summary>POST /api/Chat/sessions � Starts a new chat session.</summary>
        [HttpPost("sessions")]
        public async Task<IActionResult> StartSession([FromBody] StartSessionDTO dto)
        {
            _logger.LogInformation("User {UserId} is initiating a new chat session.", UserId);

            var session = await _chatService.StartSessionAsync(UserId, dto);

            return CreatedAtAction(
                nameof(GetSessionHistory),
                new { sessionId = session.Id },
                session);
        }

        /// <summary>GET /api/Chat/sessions � Returns paginated session history.</summary>
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _chatService.GetUserSessionsAsync(UserId, page, pageSize);
            return Ok(result);
        }

        /// <summary>GET /api/Chat/sessions/{sessionId}/history � Returns session + messages.</summary>
        [HttpGet("sessions/{sessionId:guid}/history")]
        public async Task<IActionResult> GetSessionHistory(Guid sessionId)
        {
            var history = await _chatService.GetSessionHistoryAsync(UserId, sessionId);

            if (history is null)
            {
                _logger.LogWarning("Session {SessionId} not found or unauthorized for user {UserId}.", sessionId, UserId);
                return NotFound(new { Message = "Session history not found." });
            }

            return Ok(history);
        }

        /// <summary>PUT /api/Chat/sessions/{sessionId}/end � Ends an active session.</summary>
        [HttpPut("sessions/{sessionId:guid}/end")]
        public async Task<IActionResult> EndSession(Guid sessionId)
        {
            _logger.LogInformation("User {UserId} is ending session {SessionId}.", UserId, sessionId);

            var session = await _chatService.EndSessionAsync(UserId, sessionId);
            return Ok(session);
        }

        /// <summary>DELETE /api/Chat/sessions/{sessionId} � Soft-deletes a session.</summary>
        [HttpDelete("sessions/{sessionId:guid}")]
        public async Task<IActionResult> DeleteSession(Guid sessionId)
        {
            _logger.LogInformation("User {UserId} requested soft-delete for session {SessionId}.", UserId, sessionId);

            await _chatService.DeleteSessionAsync(UserId, sessionId);

            return NoContent();
        }

        //  Message Endpoints 

        /// <summary>
        /// POST /api/Chat/send � Sends a message and returns the AI response.
        /// HTTP fallback for clients that don't support SignalR.
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] UserChatRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { Message = "Message cannot be empty." });

            _logger.LogInformation("Processing HTTP message fallback for user {UserId} in session {SessionId}.", UserId, dto.SessionId);

            var result = await _chatService.SendMessageAsync(UserId, dto);
            return Ok(result);
        }

    }
}