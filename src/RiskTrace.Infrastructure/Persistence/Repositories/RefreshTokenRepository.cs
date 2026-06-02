using Microsoft.EntityFrameworkCore;
using RiskTrace.Domain.Entities;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                refreshToken => refreshToken.TokenHash == tokenHash,
                cancellationToken);
    }

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        return dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        dbContext.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public Task RevokeAsync(
        RefreshToken refreshToken,
        DateTime revokedAt,
        Guid? replacedByTokenId = null,
        CancellationToken cancellationToken = default)
    {
        refreshToken.RevokedAt = revokedAt;
        refreshToken.ReplacedByTokenId = replacedByTokenId;

        dbContext.RefreshTokens.Update(refreshToken);

        return Task.CompletedTask;
    }
}
