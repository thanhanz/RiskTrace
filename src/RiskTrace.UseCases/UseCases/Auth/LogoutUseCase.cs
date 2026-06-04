using System.Security.Cryptography;
using System.Text;
using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Interfaces;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Request;
using RiskTrace.UseCases.Interfaces.Auth;

namespace RiskTrace.UseCases.UseCases.Auth;

public sealed class LogoutUseCase(
    IReadRepository<RefreshToken> refreshTokenReadRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    IUnitOfWork unitOfWork) : ILogoutUseCase
{
    public async Task ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var refreshToken = await refreshTokenReadRepository.FirstOrDefaultAsync(
            entity => entity.TokenHash == tokenHash,
            cancellationToken);

        if (refreshToken is null)
        {
            throw new InvalidOperationException("Invalid refresh token.");
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.UpdatedAt = DateTime.UtcNow;
        refreshToken.IsActive = false;

        await refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}
