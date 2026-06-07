using RiskTrace.Domain.Request;

namespace RiskTrace.UseCases.Interfaces.Auth;

public interface ILogoutUseCase
{
    Task ExecuteAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default);
}
