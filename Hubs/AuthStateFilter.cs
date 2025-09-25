using Microsoft.AspNetCore.SignalR;

namespace SignalRWebpack.Hubs;

public class AuthStateFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext context,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        AuthStateAttribute authFilter = (AuthStateAttribute)Attribute.GetCustomAttribute(context.HubMethod, typeof(AuthStateAttribute))!;
        if (authFilter != null)
        {
            var state = ChatHub.GetState(context.Context.ConnectionId);
            if (state != authFilter.Required)
            {
                throw new HubException("You're not authenticated for this method.");
            }
        }

        return await next(context);
    }
}