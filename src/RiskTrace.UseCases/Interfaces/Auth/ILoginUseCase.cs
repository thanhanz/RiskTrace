using RiskTrace.Core.Common;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Auth;

public interface ILoginUseCase
{
    Task<ApiResponse<AuthResponse>> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
