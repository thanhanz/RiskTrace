using System.Security.Cryptography;
using System.Text;
using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces;
using RiskTrace.Core.Interfaces.Logger;
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
    IJwtTokenService jwtTokenService,
    ILogger<LogoutUseCase> logger) : ILogoutUseCase
{
    public async Task<ApiResponse<object?>> ExecuteAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling logout request.");
        var tokenHash = HashToken(request.RefreshToken);

        var refreshToken = await refreshTokenReadRepository.FirstOrDefaultAsync(
            entity => entity.TokenHash == tokenHash,
            cancellationToken);

        if (refreshToken is null)
        {
            logger.LogWarning("Logout failed: refresh token was not found.");
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
            logger.LogInformation("Logout completed for user {UserId} without access token blacklisting.", refreshToken.UserId);
            return ApiResponse<object?>.Success(null);
        }

        var expiresAtUtc = jwtTokenService.GetTokenExpirationUtc(request.AccessToken);
        if (!expiresAtUtc.HasValue)
        {
            logger.LogWarning("Logout completed for user {UserId}, but access token expiration could not be determined.", refreshToken.UserId);
            return ApiResponse<object?>.Success(null);
        }

        var jti = jwtTokenService.GetClaim<string>(request.AccessToken, JwtClaimNames.Jti);
        if (string.IsNullOrWhiteSpace(jti))
        {
            // Legacy access tokens without a JTI cannot be blacklisted.
            logger.LogWarning("Logout completed for user {UserId}, but access token JTI was missing.", refreshToken.UserId);
            return ApiResponse<object?>.Success(null);
        }

        await tokenBlackList.AddToBlacklistAsync(
            jti,
            expiresAtUtc.Value,
            cancellationToken);

        logger.LogInformation("Logout completed for user {UserId} and access token was blacklisted.", refreshToken.UserId);

        return ApiResponse<object?>.Success(null);
    }

    private static string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}
