using GhaithAI.API.DTOs.Chat;
using GhaithAI.API.GaithAI.Application.DTOs.Chat;
using GhaithAI.API.GaithAI.Domain.Exceptions;
using GhaithAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GhaithAI.API.GaithAI.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }


        private string UserId =>
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new HubException("Unauthorized: User ID not found in token.");

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                "User {UserId} connected to ChatHub. ConnectionId: {ConnectionId}",
                UserId, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception is not null)
                _logger.LogWarning(exception,
                    "User {UserId} disconnected with error. ConnectionId: {ConnectionId}",
                    UserId, Context.ConnectionId);
            else
                _logger.LogInformation(
                    "User {UserId} disconnected cleanly. ConnectionId: {ConnectionId}",
                    UserId, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client calls this to send a message.
        /// Flow: SaveUserMsg → CallLangflow → SaveAIMsg → (SaveRiskEvent) → Push to client.
        /// </summary>
        public async Task SendMessage(UserChatRequestDto dto)
        {
            try
            {
                // Show typing indicator while AI is processing
                await Clients.Caller.SendAsync("AiTyping", true);

                var result = await _chatService.SendMessageAsync(UserId, dto);

                // Hide typing indicator
                await Clients.Caller.SendAsync("AiTyping", false);

                // Push AI message to client
                await Clients.Caller.SendAsync("ReceiveMessage", result.AiMessage);

                // Push risk alert if detected
                if (result.IsRiskDetected && result.RiskDetails is not null)
                {
                    await Clients.Caller.SendAsync("RiskAlert", new
                    {
                        result.RiskDetails.RiskType,
                        result.RiskDetails.SuggestedAction,
                        result.RiskDetails.ConfidenceScore,
                        result.IsRiskDetected
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in SendMessage for user {UserId}", UserId);

                await Clients.Caller.SendAsync("AiTyping", false);

                if (ex is UserAccountSuspendedException || ex is AiConsentRequiredException)
                {
                    throw new HubException($"Failed to process message: {ex.Message}");
                }

                throw new HubException("An error occurred while processing your message. Please try again later.");
            }
        }

        /// <summary>Client calls this to start a new session via SignalR.</summary>
        public async Task StartSession(StartSessionDTO dto)
        {
            try
            {
                var session = await _chatService.StartSessionAsync(UserId, dto);
                await Clients.Caller.SendAsync("SessionStarted", session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error starting session for user {UserId}", UserId);

                if (ex is UserAccountSuspendedException || ex is AiConsentRequiredException)
                {
                    throw new HubException($"Failed to start session: {ex.Message}");
                }
                throw new HubException("An error occurred while starting the session. Please try again later.");
            }
        }

        /// <summary>Client calls this to end a session via SignalR.</summary>
        public async Task EndSession(string sessionId)
        {
            try
            {
                if (!Guid.TryParse(sessionId, out var parsedId))
                    throw new HubException("Invalid session ID format.");

                var session = await _chatService.EndSessionAsync(UserId, parsedId);
                await Clients.Caller.SendAsync("SessionEnded", session);
            }
            catch (HubException)
            {
                throw; // re-throw HubExceptions as-is
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error ending session for user {UserId}", UserId);

                if (ex is UserAccountSuspendedException || ex is AiConsentRequiredException)
                {
                    throw new HubException($"Failed to end session: {ex.Message}");
                }

                throw new HubException("An error occurred while ending the session. Please try again later.");
            }
        }

    }
}
