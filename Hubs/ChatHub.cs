using Microsoft.AspNetCore.SignalR;

namespace SignalRWebpack.Hubs;

public class ChatHub : Hub
{
    public async Task NewMessage(string username, string message, string type) =>
        await Clients.All.SendAsync("messageReceived", username, message, "user");
}