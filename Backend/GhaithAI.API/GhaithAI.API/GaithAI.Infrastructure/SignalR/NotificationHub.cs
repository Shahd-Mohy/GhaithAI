using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace GhaithAI.API.SignalR
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        private string UserId =>
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new HubException("Unauthorized: User ID not found in token.");

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                "User {UserId} connected to NotificationHub. ConnectionId: {ConnectionId}",
                UserId, Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, UserId);
            
            // If the user is an admin, add them to the "Admin" group
            if (Context.User?.IsInRole("Admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception is not null)
                _logger.LogWarning(exception,
                    "User {UserId} disconnected from NotificationHub with error. ConnectionId: {ConnectionId}",
                    UserId, Context.ConnectionId);
            else
                _logger.LogInformation(
                    "User {UserId} disconnected cleanly from NotificationHub. ConnectionId: {ConnectionId}",
                    UserId, Context.ConnectionId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserId);

            if (Context.User?.IsInRole("Admin") == true)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admin");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
