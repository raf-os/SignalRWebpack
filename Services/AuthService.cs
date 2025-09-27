namespace SignalRWebpack.Services;

public interface IAuthService
{
    string? SaltAndHash(string password);
}

public class AuthService: IAuthService
{
    // TODO: Add logging
    public string? SaltAndHash(string password)
    {
        try
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            return hashedPassword;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}