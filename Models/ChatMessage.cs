namespace SignalRWebpack.Models;

public class ChatMessage
{
    public required string Id { get; set; }
    public required string Sender { get; set; }
    public required string Message { get; set; }
    public string? Type { get; set; }
}