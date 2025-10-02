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

public class DbUserDTO
{
    public string Name { get; set; }
    public UserAuthState Auth { get; set; }

    public DbUserDTO(string name, UserAuthState auth)
    {
        Name = name;
        Auth = auth;
    }
    public DbUserDTO(DbUser dbUser) : this(dbUser.Name, dbUser.Auth) {}
}

public class ApplicationDbContext : DbContext
{
    public DbSet<DbUser> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=users.db");
    }
}