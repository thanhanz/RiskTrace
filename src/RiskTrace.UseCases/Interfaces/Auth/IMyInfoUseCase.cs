using RiskTrace.Core.Common;
using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Auth;

public interface IMyInfoUseCase
{
    Task<ApiResponse<UserInfoResponse>> ExecuteAsync(CancellationToken cancellationToken = default);
}
