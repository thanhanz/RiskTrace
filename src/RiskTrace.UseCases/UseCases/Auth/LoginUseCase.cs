using System.Security.Cryptography;
using System.Text;
using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces;
using RiskTrace.Domain.Constants;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Auth;
using RiskTrace.UseCases.Ports.Auth;

namespace RiskTrace.UseCases.UseCases.Auth;

public sealed class LoginUseCase(
    IReadRepository<User> userReadRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : ILoginUseCase
{
    public async Task<ApiResponse<AuthResponse>> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        var user = await userReadRepository.FirstOrDefaultAsync(
            entity => entity.Email == email,
            cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponse>.Failure(
                AuthErrorCodes.InvalidCredentials,
                "Invalid email or password.");
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(refreshToken.Token),
            ExpiresAt = refreshToken.ExpiresAt,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<AuthResponse>.Success(new AuthResponse(
            AccessToken: accessToken.Token,
            RefreshToken: refreshToken.Token,
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
