using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECHO.Ticket.Business.Services.Security;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    // appsettings.json dosyasını okuyabilmek için IConfiguration'ı içeri alıyoruz
    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        // 1. Kasa Şifrelerini appsettings'ten Çek
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"]!;
        
        // 2. Mühür (İmza) Hazırlığı
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // 3. Şeffaf Zarfın İçine Konulacak Veriler (Sektörde buna "Claim" denir)
        // Kimlikte yazan bilgiler gibi düşün. Asla şifre koymuyoruz!
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("FirstName", user.FirstName), // Adını da koyduk ki ekranda "Hoşgeldin Melih" yazabilelim
            new Claim("LastName", user.LastName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        // 4. Biletin Tüm Kurallarını Belirleme
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims), // İçindeki bilgiler
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])), // Bitiş süresi
            Issuer = jwtSettings["Issuer"], // Basan
            Audience = jwtSettings["Audience"], // Hedef Kitle
            SigningCredentials = credentials // Noter Mührü
        };

        // 5. Bileti Bas ve Dışarı Ver
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token); // O uzun şifreli metni geriye döndürür
    }
}