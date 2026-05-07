using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CustomizeAuthAppJWT.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomizeAuthAppJWT.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user, out DateTime expiresAt)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("JWT key not configured");
        var issuer = jwtSection.GetValue<string>("Issuer") ?? "CustomizeAuthAppJWT";
        var audience = jwtSection.GetValue<string>("Audience") ?? "CustomizeAuthAppJWTClients";
        var expireMinutes = jwtSection.GetValue<int?>("ExpireMinutes") ?? 60;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Explicit claims: sub (user id), unique_name (username), and role
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}