using Microsoft.EntityFrameworkCore;

namespace SignalRWebpack.Database;

public enum UserAuthState
{
    Guest,
    User,
    Operator,
    Admin
}

public class DbUser
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Password { get; set; }
    public UserAuthState Auth { get; set; }
    public string? LoginToken { get; set; } // Serious security issue

    public DbUser(UserAuthState auth = UserAuthState.Guest)
    {
        Auth = auth;
    }
}

public class ApplicationDbContext : DbContext
{
    public DbSet<DbUser> Users { get; set; }
}