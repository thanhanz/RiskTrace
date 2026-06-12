using System.Security.Cryptography;
using System.Text;
using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Auth;
using RiskTrace.UseCases.Ports.Auth;

namespace RiskTrace.UseCases.UseCases.Auth;

public sealed class RefreshTokenUseCase(
    IReadRepository<RefreshToken> refreshTokenReadRepository,
    IReadRepository<User> userReadRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : IRefreshTokenUseCase
{
    public async Task<ApiResponse<AuthResponse>> ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var currentRefreshToken = await refreshTokenReadRepository.FirstOrDefaultAsync(
            entity => entity.TokenHash == tokenHash,
            cancellationToken);

        if (currentRefreshToken is null ||
            currentRefreshToken.ExpiresAt <= DateTime.UtcNow ||
            currentRefreshToken.RevokedAt is not null ||
            !currentRefreshToken.IsActive ||
            currentRefreshToken.ReplacedByTokenId is not null)
        {
            return ApiResponse<AuthResponse>.Failure(
                CommonErrors.BadRequest("Invalid refresh token."));
        }

        var user = await userReadRepository.FirstOrDefaultAsync(
            entity => entity.Id == currentRefreshToken.UserId,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ApiResponse<AuthResponse>.Failure(
                CommonErrors.NotFound("User not found."));
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(newRefreshToken.Token),
            ExpiresAt = newRefreshToken.ExpiresAt,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        currentRefreshToken.RevokedAt = DateTime.UtcNow;
        currentRefreshToken.ReplacedByTokenId = newRefreshTokenEntity.Id;
        currentRefreshToken.UpdatedAt = DateTime.UtcNow;
        currentRefreshToken.IsActive = false;

        await refreshTokenRepository.UpdateAsync(currentRefreshToken, cancellationToken);
        await refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<AuthResponse>.Success(new AuthResponse(
            AccessToken: accessToken.Token,
            RefreshToken: newRefreshToken.Token,
            User: new UserInfoResponse(
                Id: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                Role: user.Role)));
    }

    private static string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}
