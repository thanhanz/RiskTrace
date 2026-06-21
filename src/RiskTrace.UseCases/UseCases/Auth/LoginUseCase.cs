using System.Security.Cryptography;
using System.Text;
using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces;
using RiskTrace.Core.Interfaces.Logger;
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
    IUnitOfWork unitOfWork,
    ILogger<LoginUseCase> logger) : ILoginUseCase
{
    public async Task<ApiResponse<AuthResponse>> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        logger.LogInformation("Handling login request for email {Email}.", email);

        var user = await userReadRepository.FirstOrDefaultAsync(
            entity => entity.Email == email,
            cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Login failed for email {Email}: invalid credentials.", email);
            return ApiResponse<AuthResponse>.Failure(
                CommonErrors.Unauthorized("Invalid email or password."));
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

        logger.LogInformation("Login succeeded for user {UserId}.", user.Id);

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
