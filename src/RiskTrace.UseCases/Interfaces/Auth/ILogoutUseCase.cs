using RiskTrace.Core.Common;
using RiskTrace.Domain.Request;

namespace RiskTrace.UseCases.Interfaces.Auth;

public interface ILogoutUseCase
{
    Task<ApiResponse<object?>> ExecuteAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default);
}
