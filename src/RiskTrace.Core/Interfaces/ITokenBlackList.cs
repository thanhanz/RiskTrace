namespace RiskTrace.Core.Interfaces;

public interface ITokenBlackList
{
    Task AddToBlacklistAsync(
        string accessToken,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}
