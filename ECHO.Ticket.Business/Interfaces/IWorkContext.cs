namespace ECHO.Ticket.Business.Interfaces;

public interface IWorkContext
{
    // O an sisteme giriş yapmış kullanıcının ID'sini tutacak
    Guid UserId { get; } 
    // İleride buraya Email, Role, FullName gibi özellikleri de ekleyebiliriz
}