using RiskTrace.Domain.Entities;

namespace RiskTrace.UseCases.Ports.Auth;

public interface IJwtTokenService
{
    TokenResult GenerateAccessToken(User user);

    TokenResult GenerateRefreshToken();

    DateTime? GetAccessTokenExpirationUtc(string accessToken);
}
