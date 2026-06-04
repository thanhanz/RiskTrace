using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Auth;

public interface IRegisterUseCase
{
    Task<AuthResponse> ExecuteAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);
}
