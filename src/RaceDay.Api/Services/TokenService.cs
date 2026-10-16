using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Models;

namespace RaceDay.Api.Services;
public class TokenService(IConfiguration configuration)
{
    public (string Token, DateTime ExpiresAtUtc) Create(User user)
    {
        var keyText = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured.");
        var expires = DateTime.UtcNow.AddHours(2);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role!.RoleName),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(configuration["Jwt:Issuer"] ?? "RaceDay",
            configuration["Jwt:Audience"] ?? "RaceDayClients", claims, expires: expires, signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
