namespace ECHO.Ticket.Business.Interfaces;

public interface IPasswordHasher
{
    // Düz metin şifreyi alır, kırılamaz karmaşık metne çevirir
    string HashPassword(string password);

    // Giriş yaparken, girilen şifre ile veritabanındaki karmaşık şifre uyuşuyor mu diye bakar
    bool VerifyPassword(string password, string passwordHash);
}