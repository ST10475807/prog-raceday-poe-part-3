
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Models;

namespace RaceDay.Api.Services;

public interface ITokenService
{
    (string Token, DateTime Expires) Create(User user);
}

public class TokenService(IConfiguration configuration) : ITokenService
{
    public (string Token, DateTime Expires) Create(User user)
    {
        var section = configuration.GetSection("Jwt");

        var issuer = section["Issuer"];
        var audience = section["Audience"];
        var secret = section["Key"];

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException(
                "JWT signing key is missing from configuration.");
        }

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "JWT Issuer or Audience is missing from configuration.");
        }

        // Create the signing key from the configured secret.
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(2);

        // Create user identity and role claims.
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role),

            new Claim(
                "firstName",
                user.FirstName)
        };

        // Generate the JWT token.
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString =
            new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, expires);
    }
}
