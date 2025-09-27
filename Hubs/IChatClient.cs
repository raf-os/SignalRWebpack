using SignalRWebpack.Models;

namespace SignalRWebpack.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(string user, string message, string type);
    Task SystemMessage(string message);
    Task LogIn(string username);
    Task UpdateClientList(List<User> users);
}