using ECHO.Ticket.Core.Entities; 

namespace ECHO.Ticket.Business.Interfaces;

public interface IJwtProvider
{
    // İçeriye bir Kullanıcı nesnesi alacak ve geriye şifrelenmiş, upuzun bir metin (Token) dönecek.
    string GenerateToken(User user); 
}