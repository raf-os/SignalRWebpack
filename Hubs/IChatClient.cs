using SignalRWebpack.Models;

namespace SignalRWebpack.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(string user, string message, string type);
    Task Register(string username);
    Task UpdateClientList(List<User> users);
}