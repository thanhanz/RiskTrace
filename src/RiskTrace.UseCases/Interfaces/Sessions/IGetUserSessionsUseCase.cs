using RiskTrace.Core.Common;
using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Sessions;

public interface IGetUserSessionsUseCase
{
    Task<ApiResponse<IReadOnlyList<SessionResponse>>> ExecuteAsync(CancellationToken cancellationToken = default);
}
