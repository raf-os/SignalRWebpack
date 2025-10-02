using Microsoft.EntityFrameworkCore;
using SignalRWebpack.Database;

namespace SignalRWebpack.Endpoints;

public class TokenRequest
{
    public string? LoginToken { get; set; }
}

public static class ApiEndpoints
{
    private static string _rootPath = "/api";
    public static void Map(WebApplication app)
    {
        app.MapPost(_rootPath + "/validateToken", ValidateToken);
        app.MapPost(_rootPath + "/fetchUserDb", FetchUserList);
    }

    private static async Task<bool> CheckHeaders(HttpRequest httpRequest, UserAuthState requiredAuth)
    {
        using var context = new ApplicationDbContext();
        string? authHeader = httpRequest.Headers["Auth-Token"];
        if (authHeader == null)
        {
            return false;
        }
        DbUser? user = await context.Users.Where(u => u.LoginToken == authHeader).FirstOrDefaultAsync();
        if (user == null)
        {
            return false;
        }
        if (user.Auth < requiredAuth)
        {
            return false;
        }

        return true;
    }

    static async Task<IResult> ValidateToken(TokenRequest tokenRequest)
    {
        using var context = new ApplicationDbContext();
        if (tokenRequest == null || tokenRequest.LoginToken == null)
        {
            return TypedResults.Unauthorized();
        }
        DbUser? user = await context.Users.Where(u => u.LoginToken == tokenRequest.LoginToken).FirstOrDefaultAsync();
        if (user == null)
        {
            return TypedResults.Unauthorized();
        }
        return TypedResults.Ok(new { Auth = user.Auth });
    }

    static async Task<IResult> FetchUserList(HttpRequest httpRequest)
    {
        bool isValid = await CheckHeaders(httpRequest, UserAuthState.Operator);
        if (!isValid)
        {
            return TypedResults.Unauthorized();
        }
        using var context = new ApplicationDbContext();
        var users = await context.Users.Select(x => new DbUserDTO(x)).ToListAsync();

        return TypedResults.Ok(users);
    }
}