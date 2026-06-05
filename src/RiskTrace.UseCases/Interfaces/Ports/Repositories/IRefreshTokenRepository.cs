using RiskTrace.Domain.Entities;

namespace RiskTrace.UseCases.Ports.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task RevokeAsync(
        RefreshToken refreshToken,
        DateTime revokedAt,
        Guid? replacedByTokenId = null,
        CancellationToken cancellationToken = default);
}
