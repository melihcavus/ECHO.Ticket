using System.Security.Claims;
using ECHO.Ticket.Business.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECHO.Ticket.Business.Security;

public class WorkContext : IWorkContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // .NET'in o anki HTTP isteğine (ve dolayısıyla Token'a) ulaşmamızı sağlayan aracını içeri alıyoruz
    public WorkContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            // Token'ın içindeki (Zarftaki) "Sub" veya "NameIdentifier" etiketini bulup okuyoruz
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            // Eğer token yoksa veya boşsa, boş bir Guid dön (Zaten [Authorize] kapısı buna izin vermez ama güvenlik önlemi)
            if (string.IsNullOrEmpty(userIdString))
            {
                return Guid.Empty; 
            }

            // Metin olarak aldığımız ID'yi Guid formatına çevirip teslim ediyoruz
            return Guid.Parse(userIdString);
        }
    }
}