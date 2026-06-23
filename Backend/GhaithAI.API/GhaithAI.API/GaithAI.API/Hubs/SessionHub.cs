using Microsoft.AspNetCore.SignalR;

namespace GhaithAI.API.GaithAI.API.Hubs;

public class SessionHub : Hub
{
    public async Task JoinSession(string sessionId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

    public async Task SendOffer(string sessionId, string sdp)
        => await Clients.OthersInGroup(sessionId).SendAsync("ReceiveOffer", new { sessionId, sdp });

    public async Task SendAnswer(string sessionId, string sdp)
        => await Clients.OthersInGroup(sessionId).SendAsync("ReceiveAnswer", new { sessionId, sdp });

    public async Task SendIceCandidate(string sessionId, object candidate)
        => await Clients.OthersInGroup(sessionId).SendAsync("ReceiveIceCandidate", new { sessionId, candidate });
}