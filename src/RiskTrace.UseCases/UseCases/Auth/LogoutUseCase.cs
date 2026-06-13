using System.Security.Cryptography;
using System.Text;
using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces;
using RiskTrace.Domain.Constants;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Request;
using RiskTrace.UseCases.Interfaces.Auth;
using RiskTrace.UseCases.Ports.Auth;

namespace RiskTrace.UseCases.UseCases.Auth;

public sealed class LogoutUseCase(
    IReadRepository<RefreshToken> refreshTokenReadRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ITokenBlackList tokenBlackList,
    IJwtTokenService jwtTokenService) : ILogoutUseCase
{
    public async Task<ApiResponse<object?>> ExecuteAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var refreshToken = await refreshTokenReadRepository.FirstOrDefaultAsync(
            entity => entity.TokenHash == tokenHash,
            cancellationToken);

        if (refreshToken is null)
        {
            return ApiResponse<object?>.Failure(
                CommonErrors.BadRequest("Invalid refresh token."));
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.UpdatedAt = DateTime.UtcNow;
        refreshToken.IsActive = false;

        await refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return ApiResponse<object?>.Success(null);
        }

        var expiresAtUtc = jwtTokenService.GetTokenExpirationUtc(request.AccessToken);
        if (!expiresAtUtc.HasValue)
        {
            return ApiResponse<object?>.Success(null);
        }

        var jti = jwtTokenService.GetClaim<string>(request.AccessToken, JwtClaimNames.Jti);
        if (string.IsNullOrWhiteSpace(jti))
        {
            // Legacy access tokens without a JTI cannot be blacklisted.
            return ApiResponse<object?>.Success(null);
        }

        await tokenBlackList.AddToBlacklistAsync(
            jti,
            expiresAtUtc.Value,
            cancellationToken);

        return ApiResponse<object?>.Success(null);
    }

    private static string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}
