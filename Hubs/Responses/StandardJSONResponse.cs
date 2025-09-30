namespace SignalRWebpack.Hubs.Responses;

public class StandardJsonResponse : IBaseResponse
{
    public required bool Success { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}