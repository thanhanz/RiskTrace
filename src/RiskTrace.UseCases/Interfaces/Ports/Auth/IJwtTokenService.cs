using RiskTrace.Domain.Constants;
using RiskTrace.Domain.Entities;

namespace RiskTrace.UseCases.Ports.Auth;

public interface IJwtTokenService
{
    TokenResult GenerateAccessToken(User user);

    TokenResult GenerateRefreshToken();

    DateTime? GetTokenExpirationUtc(string token);

    T? GetClaim<T>(string token, string claimType);
}
