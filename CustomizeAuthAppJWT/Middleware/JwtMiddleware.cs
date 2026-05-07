using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomizeAuthAppJWT.Middleware;

// Manual token extraction + validation middleware
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public JwtMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract Authorization header
        if (context.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
        {
            var header = authHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(header) && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = header.Substring("Bearer ".Length).Trim();

                var validatedPrincipal = ValidateToken(token);
                if (validatedPrincipal != null)
                {
                    // Manually set the ClaimsPrincipal on HttpContext.User
                    context.User = validatedPrincipal;
                }
                else
                {
                    // Invalid token -> leave context.User unauthenticated
                }
            }
        }

        await _next(context);
    }

    // Explicit validation using JwtSecurityTokenHandler and TokenValidationParameters
    private ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("JWT key not configured");
            var issuer = jwtSection.GetValue<string>("Issuer");
            var audience = jwtSection.GetValue<string>("Audience");

            var tokenHandler = new JwtSecurityTokenHandler();

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = !string.IsNullOrEmpty(issuer),
                ValidIssuer = issuer,
                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // strict expiration
            };

            var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

            // Ensure token uses expected algorithm (HMAC-SHA256)
            if (validatedToken is JwtSecurityToken jwt &&
                jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            {
                return principal;
            }

            return null;
        }
        catch (SecurityTokenExpiredException)
        {
            // Token expired
            return null;
        }
        catch
        {
            // invalid token or validation failed
            return null;
        }
    }
}