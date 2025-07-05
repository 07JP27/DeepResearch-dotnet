using Microsoft.AspNetCore.SignalR;

namespace DeepResearch.Web.Hubs;

public class ResearchHub : Hub
{
    public async Task JoinGroup(string clientId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, clientId);
    }

    public async Task LeaveGroup(string clientId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, clientId);
    }
}
