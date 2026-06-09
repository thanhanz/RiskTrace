using RiskTrace.Core.Common;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Auth;

public interface IRefreshTokenUseCase
{
    Task<ApiResponse<AuthResponse>> ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);
}
