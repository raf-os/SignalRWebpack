using SignalRWebpack.Hubs;
using SignalRWebpack.Models;

namespace SignalRWebpack.Models;

public class User
{
    public required string Id { get; set; }
    public string? Name { get; set; }
    public AuthState authState { get; set; }
}