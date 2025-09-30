namespace SignalRWebpack.Services;

public interface IAuthService
{
    string? SaltAndHash(string password);
    bool CheckValidity(string password, string hash);
    Guid GenerateUuid();
}

public class AuthService : IAuthService
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

    public bool CheckValidity(string password, string hash)
    {
        bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        return isValid;
    }

    public Guid GenerateUuid()
    {
        return Guid.NewGuid();
    }
}