namespace SignalRWebpack.Hubs.Responses;

public interface IBaseResponse
{
    public Boolean Success { get; set; }
}

public class BaseResponse<T> : IBaseResponse
{
    public required bool Success { get; set; }
    public string? Message { get; set; }
    public T Metadata { get; set; } = default!;
}