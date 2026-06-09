namespace RiskTrace.Core.Interfaces;

public interface ITokenBlackList
{
    Task AddToBlacklistAsync(
        string jti,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string jti,
        CancellationToken cancellationToken = default);
}
