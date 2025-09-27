using Microsoft.AspNetCore.SignalR;
using SignalRWebpack.Models;
using SignalRWebpack.Database;
using Microsoft.EntityFrameworkCore;
using SignalRWebpack.Services;
using SignalRWebpack.Hubs.Responses;

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

public class RegisterRequestObject
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class ChatHub(IAuthService authService) : Hub<IChatClient>
{
    private static readonly List<User> States = [];
    private readonly IAuthService _authService = authService;

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

    public override async Task OnConnectedAsync()
    {
        User newUser = new() { Id = Context.ConnectionId };
        States.Add(newUser);
        await PushClientListUpdate();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.ConnectionId != null)
        {
            User? query = States.Find(s => s.Id == Context.ConnectionId);
            if (query != null) { States.Remove(query); }
            await PushClientListUpdate();
        }
        await base.OnDisconnectedAsync(exception);
    }

    [AuthState(AuthState.User)]
    public async Task NewMessage(string username, string message, string type)
    {
        await Clients.All.ReceiveMessage(username, message, type);
    }

    public async Task LogIn(string username)
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
        await Clients.Caller.LogIn(username);
        await FetchUsers();
    }

    public async Task<StandardJsonResponse> Register(RegisterRequestObject registerRequest)
    {
        using var context = new ApplicationDbContext();
        var lookupUsername = await context.Users.Where(u => u.Name == registerRequest.Username).ToListAsync();
        if (lookupUsername.Count != 0)
        {
            return new StandardJsonResponse { Success = false, Message = "Username already exists." };
        }

        var saltedPassword = _authService.SaltAndHash(registerRequest.Password);

        if (saltedPassword == null)
        {
            return new StandardJsonResponse { Success = false, Message = "Error encrypting password. Please choose a different one." };
        }

        context.Users.Add(new DbUser { Name = registerRequest.Username, Password = saltedPassword });

        await context.SaveChangesAsync();

        return new StandardJsonResponse { Success = true, Message = "User registered successfully!" };
    }

    public async Task FetchUsers()
    {
        var loggedUsers = GetLoggedUsers();
        await Clients.Caller.UpdateClientList(loggedUsers);
    }
}