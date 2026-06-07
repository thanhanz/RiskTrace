namespace RiskTrace.UseCases.Ports.Auth;

public interface IAccessTokenRevocationService
{
    Task RevokeAsync(
        string accessToken,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);

    Task<bool> IsRevokedAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}
