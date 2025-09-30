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

    private static User? GetUserState(string connectionId)
    {
        User? uState = States.Find(u => u.Id == connectionId);
        if (uState == null)
        {
            return null;
        }
        return uState;
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

    public async Task<StandardJsonResponse> LogIn(string username, string password)
    {
        using var context = new ApplicationDbContext();
        DbUser? user = await context.Users.SingleAsync(u => u.Name == username);

        if (user == null)
        {
            return new StandardJsonResponse { Success = false, Message = "Invalid credentials." };
        }

        bool isValid = _authService.CheckValidity(password, user.Password);

        if (!isValid)
        {
            return new StandardJsonResponse { Success = false, Message = "Invalid credentials." };
        }

        string token = Guid.NewGuid().ToString();

        user.LoginToken = token;

        await context.SaveChangesAsync();

        User? uState = States.Find(u => u.Id == Context.ConnectionId);
        if (uState == null)
        {
            return new StandardJsonResponse { Success = false, Message = "Unknown error occurred." };
        }

        uState.Name = username;
        uState.authState = AuthState.User;

        var metadata = new Dictionary<string, string>
        {
            { "Token", token },
            { "Username", username },
            { "ConnectionId", Context.ConnectionId }
        };

        await PushClientListUpdate();

        return new StandardJsonResponse { Success = true, Metadata = metadata };
    }

    public async Task<StandardJsonResponse> ReLogIn(string token)
    {
        using var context = new ApplicationDbContext();
        try
        {
            var user = await context.Users.Where(u => u.LoginToken == token).SingleAsync();
            if (user == null || user.LoginToken == null)
            {
                return new StandardJsonResponse { Success = false };
            }

            var metadata = new Dictionary<string, string>
            {
                { "Token", user.LoginToken },
                { "Username", user.Name },
                { "ConnectionId", Context.ConnectionId }
            };

            var u = GetUserState(Context.ConnectionId);
            if (u == null)
            {
                return new StandardJsonResponse { Success = false, Message = "Unknown error occurred." };
            }

            u.Name = user.Name;
            u.authState = AuthState.User;

            await PushClientListUpdate();

            return new StandardJsonResponse { Success = true, Metadata = metadata };
        }
        catch (Exception)
        {
            return new StandardJsonResponse { Success = false, Message = "Unknown error occurred." };
        }
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

        return new StandardJsonResponse { Success = true };
    }

    public async Task LogOut()
    {
        var cId = Context.ConnectionId;
        var uState = GetUserState(cId);

        if (uState != null)
        {
            uState.Name = null;
            uState.authState = AuthState.Guest;
        }

        await PushClientListUpdate();
    }

    public async Task FetchUsers()
    {
        var loggedUsers = GetLoggedUsers();
        await Clients.Caller.UpdateClientList(loggedUsers);
    }
}