using UserService.Application.Services.Interfaces;

namespace UserService.Application.Services;

public class PasswordEncripter : IPasswordEncripter
{
    public string Encrypt(string password)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));
        
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hashedPassword)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));
        
        if (hashedPassword == null)
            throw new ArgumentNullException(nameof(hashedPassword));
        
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;
        
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch (Exception)
        {
            return false;
        }
    }
}