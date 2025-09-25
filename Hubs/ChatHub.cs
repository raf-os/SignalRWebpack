using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace SignalRWebpack.Hubs;

public enum AuthState
{
    Guest,
    User
}

[AttributeUsage(AttributeTargets.Method)]
public class AuthStateAttribute : Attribute
{
    public AuthState Required { get; set; } = AuthState.Guest;
    public AuthStateAttribute(AuthState required = AuthState.Guest) =>
        Required = required;
}

public class ChatHub : Hub<IChatClient>
{
    private static readonly ConcurrentDictionary<string, AuthState> States = new();

    public static AuthState GetState(string connectionId)
    {
        return States.TryGetValue(connectionId, out var state)
            ? state
            : AuthState.Guest;
    }

    public override Task OnConnectedAsync()
    {
        States[Context.ConnectionId] = AuthState.Guest;
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        States.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }

    [AuthState(AuthState.User)]
    public async Task NewMessage(string username, string message, string type)
    {
        await Clients.All.ReceiveMessage(username, message, type);
    }

    public async Task Register(string username)
    {
        States[Context.ConnectionId] = AuthState.User;
        await Clients.Caller.Register(username);
    }
}