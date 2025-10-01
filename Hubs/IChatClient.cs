using SignalRWebpack.Models;

namespace SignalRWebpack.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(ChatMessage chatMessage);
    Task SystemMessage(string message);
    Task LogIn(string username, string password);
    Task ReLogIn(string token);
    Task LogOut();
    Task Register(RegisterRequestObject registerRequest);
    Task UpdateClientList(List<User> users);
}