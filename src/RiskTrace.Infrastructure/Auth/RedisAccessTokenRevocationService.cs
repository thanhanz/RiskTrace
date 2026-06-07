using Microsoft.Extensions.Caching.Distributed;
using RiskTrace.UseCases.Ports.Auth;

namespace RiskTrace.Infrastructure.Auth;

public sealed class RedisAccessTokenRevocationService(IDistributedCache distributedCache) : IAccessTokenRevocationService
{
    private const string RevokedValue = "revoked";
    private const string KeyPrefix = "auth:revoked-access-token:";

    public async Task RevokeAsync(
        string accessToken,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        var ttl = expiresAtUtc - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            return;
        }

        await distributedCache.SetStringAsync(
            BuildKey(accessToken),
            RevokedValue,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            },
            cancellationToken);
    }

    public async Task<bool> IsRevokedAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var cachedValue = await distributedCache.GetStringAsync(
            BuildKey(accessToken),
            cancellationToken);

        return !string.IsNullOrWhiteSpace(cachedValue);
    }

    private static string BuildKey(string accessToken) => $"{KeyPrefix}{accessToken}";
}
