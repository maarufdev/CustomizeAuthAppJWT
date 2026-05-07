using CustomizeAuthAppJWT.Models;

namespace CustomizeAuthAppJWT.Services;

public interface ITokenService
{
    string GenerateToken(User user, out DateTime expiresAt);
}