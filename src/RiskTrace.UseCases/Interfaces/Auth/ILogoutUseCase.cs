using RiskTrace.Domain.Request;

namespace RiskTrace.UseCases.Interfaces.Auth;

public interface ILogoutUseCase
{
    Task ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);
}
