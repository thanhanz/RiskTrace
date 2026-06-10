using RiskTrace.Core.Common;
using RiskTrace.Domain.Constants;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Auth;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Auth;

public sealed class MyInfoUseCase(
    ICurrentUserProvider currentUserProvider,
    IUserRepository userRepository) : IMyInfoUseCase
{
    public async Task<ApiResponse<UserInfoResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (currentUserProvider.UserId is not { } userId)
        {
            return ApiResponse<UserInfoResponse>.Failure(
                AuthErrorCodes.UserNotFound,
                "User not found.");
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ApiResponse<UserInfoResponse>.Failure(
                AuthErrorCodes.UserNotFound,
                "User not found.");
        }

        return ApiResponse<UserInfoResponse>.Success(new UserInfoResponse(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role));
    }
}
