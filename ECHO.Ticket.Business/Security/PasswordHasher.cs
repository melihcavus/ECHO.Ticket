using ECHO.Ticket.Business.Interfaces;


namespace ECHO.Ticket.Business.Security;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        // Kullanıcının girdiği düz şifre ile veritabanındaki hash'i karşılaştırır
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}