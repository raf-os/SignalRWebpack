using Microsoft.EntityFrameworkCore;
using SignalRWebpack.Database;

namespace SignalRWebpack.Endpoints;

public class TokenRequest
{
    public string? LoginToken { get; set; }
}

public class UpdateUserDataRequest
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public UserAuthState UserAuth { get; set; }
}

public class CheckHeaderResult
{
    public bool isValid { get; set; }
    public UserAuthState? authState { get; set; }
    public static CheckHeaderResult Invalid = new CheckHeaderResult { isValid = false };
}

public static class ApiEndpoints
{
    private static string _rootPath = "/api";
    public static void Map(WebApplication app)
    {
        app.MapPost(_rootPath + "/validateToken", ValidateToken);
        app.MapPost(_rootPath + "/fetchUserDb", FetchUserList);
        app.MapPost(_rootPath + "/updateUserData", UpdateUserData);
    }

    private static async Task<CheckHeaderResult> CheckHeaders(HttpRequest httpRequest, UserAuthState requiredAuth)
    {
        using var context = new ApplicationDbContext();
        string? authHeader = httpRequest.Headers["Auth-Token"];
        if (authHeader == null)
        {
            return CheckHeaderResult.Invalid;
        }
        DbUser? user = await context.Users.Where(u => u.LoginToken == authHeader).FirstOrDefaultAsync();
        if (user == null)
        {
            return CheckHeaderResult.Invalid;
        }
        if (user.Auth < requiredAuth)
        {
            return CheckHeaderResult.Invalid;
        }

        return new CheckHeaderResult { isValid = true, authState = user.Auth };
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
        var result = await CheckHeaders(httpRequest, UserAuthState.Operator);
        if (!result.isValid)
        {
            return TypedResults.Unauthorized();
        }
        using var context = new ApplicationDbContext();
        var users = await context.Users.Select(x => new DbUserDTO(x)).ToListAsync();

        return TypedResults.Ok(users);
    }

    static async Task<IResult> UpdateUserData(HttpRequest httpRequest, UpdateUserDataRequest newData)
    {
        var result = await CheckHeaders(httpRequest, UserAuthState.Operator);
        if (!result.isValid)
        {
            return TypedResults.Unauthorized();
        }
        using var context = new ApplicationDbContext();

        var userToUpdate = await context.Users.Where(u => u.Id == newData.UserId).FirstOrDefaultAsync();

        if (userToUpdate == null)
        {
            return TypedResults.NotFound();
        }

        if (userToUpdate.Auth < result.authState)
        {
            return TypedResults.Unauthorized();
        }

        userToUpdate.Auth = newData.UserAuth;
        await context.SaveChangesAsync();

        return TypedResults.Ok();
    }
}