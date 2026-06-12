using RiskTrace.Core.Common;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Sessions;

public interface ICreateSessionUseCase
{
    Task<ApiResponse<SessionResponse>> ExecuteAsync(
        CreateSessionRequest request,
        CancellationToken cancellationToken = default);
}
