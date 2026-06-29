using Microsoft.AspNetCore.SignalR;

namespace GhaithAI.API.GaithAI.API.Hubs;

public class SessionHub : Hub
{
    public async Task JoinSession(string sessionId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, (sessionId ?? "").ToLowerInvariant());

    public async Task PatientReady(string sessionId)
        => await Clients.OthersInGroup((sessionId ?? "").ToLowerInvariant()).SendAsync("PatientReady", new { sessionId = (sessionId ?? "").ToLowerInvariant() });

    public async Task SendOffer(string sessionId, string sdp)
        => await Clients.OthersInGroup((sessionId ?? "").ToLowerInvariant()).SendAsync("ReceiveOffer", new { sessionId = (sessionId ?? "").ToLowerInvariant(), sdp });

    public async Task SendAnswer(string sessionId, string sdp)
        => await Clients.OthersInGroup((sessionId ?? "").ToLowerInvariant()).SendAsync("ReceiveAnswer", new { sessionId = (sessionId ?? "").ToLowerInvariant(), sdp });

    public async Task DoctorJoined(string sessionId)
        => await Clients.OthersInGroup((sessionId ?? "").ToLowerInvariant()).SendAsync("DoctorJoined", new { sessionId = (sessionId ?? "").ToLowerInvariant() });

    public async Task SendIceCandidate(string sessionId, object candidate)
        => await Clients.OthersInGroup((sessionId ?? "").ToLowerInvariant()).SendAsync("ReceiveIceCandidate", new { sessionId = (sessionId ?? "").ToLowerInvariant(), candidate });
}
