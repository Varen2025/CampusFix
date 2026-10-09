using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusFix.Api.Models;
using CampusFix.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace CampusFix.Api.Services;

public sealed class TokenService(IConfiguration configuration)
{
    public AuthResponse Create(AppUser user)
    {
        var expires = DateTimeOffset.UtcNow.AddHours(2);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"] ?? "CampusFix",
            configuration["Jwt:Audience"] ?? "CampusFix",
            claims, notBefore: DateTime.UtcNow, expires: expires.UtcDateTime, signingCredentials: credentials);
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expires,
            new UserSummary(user.Id, user.FullName, user.Email, user.Role));
    }
}
