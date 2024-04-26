using Microsoft.AspNetCore.SignalR;
namespace MatchinX.API.Hubs;

public class MatchingHub : Hub
{
    public Task ConnectToMacthingEngine(string jwt)
    {
        Groups.AddToGroupAsync(Context.ConnectionId, "CrudMessage");

        return Task.CompletedTask;
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.Caller.SendAsync("ReceiveMessage", user, message);
    }
}
