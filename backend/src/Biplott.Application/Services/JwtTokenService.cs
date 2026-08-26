using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Biplott.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Biplott.Application.Services;

public interface IJwtTokenService
{
    (string Token, int ExpiresInSeconds) GenerateAccessToken(string userId, string email, string displayName, IEnumerable<string> roles);
    string GenerateRefreshToken();
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, int ExpiresInSeconds) GenerateAccessToken(
        string userId,
        string email,
        string displayName,
        IEnumerable<string> roles)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? "BiplotSuperSecretKeyForDevelopmentPhase3_MustBeAtLeast32BytesLong!";
        var issuer = _configuration["Jwt:Issuer"] ?? "BiplottApi";
        var audience = _configuration["Jwt:Audience"] ?? "BiplottClient";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var m) ? m : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new("displayName", displayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var expiresInSeconds = (int)(expires - DateTime.UtcNow).TotalSeconds;

        return (tokenString, expiresInSeconds);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
