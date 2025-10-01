using SignalRWebpack.Database;

namespace SignalRWebpack.Hubs.Responses;

public class LoginMetadata
{
    public required string Token { get; set; }
    public required string Username { get; set; }
    public required string ConnectionId { get; set; }
    public required UserAuthState Auth { get; set; }
}

public class LoginMetadataResponse : BaseResponse<LoginMetadata>
{
}