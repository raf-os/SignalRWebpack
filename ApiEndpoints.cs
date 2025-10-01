namespace SignalRWebpack.Endpoints;

public static class ApiEndpoints
{
    private static string _rootPath = "/api/";
    public static void Map(WebApplication app)
    {
        app.MapGet(_rootPath + "", async context =>
        {
            await context.Response.WriteAsJsonAsync(new { Message = "API endpoint working correctly." });
        });
    }
}