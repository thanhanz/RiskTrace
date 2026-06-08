using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Auth;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Auth;

public sealed class MyInfoUseCase(
    ICurrentUserProvider currentUserProvider,
    IUserRepository userRepository) : IMyInfoUseCase
{
    public async Task<MyInfoResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (currentUserProvider.UserId is not { } userId)
        {
            return new MyInfoResult(
                IsAuthenticated: false,
                User: null);
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return new MyInfoResult(
                IsAuthenticated: true,
                User: null);
        }

        return new MyInfoResult(
            IsAuthenticated: true,
            User: new UserInfoResponse(
                Id: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                Role: user.Role));
    }
}
