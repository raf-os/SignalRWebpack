using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SignalRWebpack.Models;

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
    private static readonly List<User> States = [];

    public static AuthState GetState(string connectionId)
    {
        User? query = States.Find(s => s.Id == connectionId);
        return (query == null) ? AuthState.Guest : query.authState;
    }

    private static List<User> GetLoggedUsers()
    {
        var loggedUsers = States.FindAll(s => s.authState == AuthState.User);
        return loggedUsers;
    }

    private Task PushClientListUpdate()
    {
        var loggedUsers = GetLoggedUsers();
        return Clients.All.UpdateClientList(loggedUsers);
    }

    public override Task OnConnectedAsync()
    {
        User newUser = new() { Id = Context.ConnectionId };
        States.Add(newUser);
        PushClientListUpdate();
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.ConnectionId != null)
        {
            User? query = States.Find(s => s.Id == Context.ConnectionId);
            if (query != null) { States.Remove(query); }
            PushClientListUpdate();
        }
        return base.OnDisconnectedAsync(exception);
    }

    [AuthState(AuthState.User)]
    public async Task NewMessage(string username, string message, string type)
    {
        await Clients.All.ReceiveMessage(username, message, type);
    }

    public async Task Register(string username)
    {
        User? guestUser = States.Find(s => s.Id == Context.ConnectionId);

        if (guestUser == null)
        {
            throw new HubException("Error: No user ID!");
        }

        bool isInvalid = States.Exists(s => s.Name == username);

        if (isInvalid)
        {
            await Clients.Caller.ReceiveMessage("sys", "Username already exists.", "server");
            return;
        }

        guestUser.Name = username;
        guestUser.authState = AuthState.User;
        await Clients.Caller.Register(username);
        await FetchUsers();
    }

    public async Task FetchUsers()
    {
        var loggedUsers = GetLoggedUsers();
        await Clients.Caller.UpdateClientList(loggedUsers);
    }
}