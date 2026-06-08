using Microsoft.Extensions.Caching.Distributed;
using RiskTrace.Core.Interfaces;

namespace RiskTrace.Infrastructure.Auth;

public sealed class TokenBlackList(IDistributedCache distributedCache) : ITokenBlackList
{
    private const string BlacklistValue = "revoked";
    private const string KeyPrefix = "auth:token-blacklist:";

    public async Task AddToBlacklistAsync(
        string jti,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        var ttl = expiresAtUtc - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            return;
        }

        await distributedCache.SetStringAsync(
            BuildKey(jti),
            BlacklistValue,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            },
            cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string jti,
        CancellationToken cancellationToken = default)
    {
        var cachedValue = await distributedCache.GetStringAsync(
            BuildKey(jti),
            cancellationToken);

        return !string.IsNullOrWhiteSpace(cachedValue);
    }

    private static string BuildKey(string jti) => $"{KeyPrefix}{jti}";
}
