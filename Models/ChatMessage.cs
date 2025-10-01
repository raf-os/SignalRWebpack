namespace SignalRWebpack.Models;

public class ChatMessage
{
    public required string Id;
    public required string Sender;
    public required string Message;
    public string? Type;
}