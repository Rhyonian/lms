using BCryptNet = BCrypt.Net.BCrypt;

namespace Lms.Api.Services;

public class PasswordHasher
{
    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCryptNet.HashPassword(password);
    }

    public bool Verify(string password, string? passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        return BCryptNet.Verify(password, passwordHash);
    }
}
