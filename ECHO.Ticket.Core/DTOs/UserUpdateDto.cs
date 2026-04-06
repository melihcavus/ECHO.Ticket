namespace ECHO.Ticket.Core.DTOs;

public class UserUpdateDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    // Şifre ve E-posta güncelleme işlemleri güvenlik gereği ayrı metodlarda yapılır, buraya koymuyoruz.
}