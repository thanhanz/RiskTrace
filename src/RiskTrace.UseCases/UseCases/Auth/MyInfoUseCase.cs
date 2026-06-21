using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces.Logger;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Auth;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Auth;

public sealed class MyInfoUseCase(
    ICurrentUserProvider currentUserProvider,
    IUserRepository userRepository,
    ILogger<MyInfoUseCase> logger) : IMyInfoUseCase
{
    public async Task<ApiResponse<UserInfoResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling my-info request.");
        if (currentUserProvider.UserId is not { } userId)
        {
            logger.LogWarning("My-info request failed: user is not authenticated.");
            return ApiResponse<UserInfoResponse>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            logger.LogWarning("My-info request failed for user {UserId}: user not found or inactive.", userId);
            return ApiResponse<UserInfoResponse>.Failure(
                CommonErrors.NotFound("User not found."));
        }

        logger.LogInformation("My-info request succeeded for user {UserId}.", user.Id);

        return ApiResponse<UserInfoResponse>.Success(new UserInfoResponse(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role));
    }
}
