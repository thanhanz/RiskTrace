using System.Security.Cryptography;
using System.Text;
using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces;
using RiskTrace.Core.Interfaces.Logger;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Enums;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Auth;
using RiskTrace.UseCases.Ports.Auth;

namespace RiskTrace.UseCases.UseCases.Auth;

public sealed class RegisterUseCase(
    IReadRepository<User> userReadRepository,
    IRepository<User> userRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ILogger<RegisterUseCase> logger) : IRegisterUseCase
{
    public async Task<ApiResponse<AuthResponse>> ExecuteAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var fullName = request.FullName.Trim();
        logger.LogInformation("Handling register request for email {Email}.", email);

        var existingUser = await userReadRepository.FirstOrDefaultAsync(
            user => user.Email == email,
            cancellationToken);

        if (existingUser is not null)
        {
            logger.LogWarning("Register request failed for email {Email}: email already exists.", email);
            return ApiResponse<AuthResponse>.Failure(
                CommonErrors.Conflict("Email already exists."));
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = fullName,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = UserRole.CUSTOMER,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

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

        await userRepository.AddAsync(user, cancellationToken);
        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Register request succeeded for user {UserId}.", user.Id);

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
