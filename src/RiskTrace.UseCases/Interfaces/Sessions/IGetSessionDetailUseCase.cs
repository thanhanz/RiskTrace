using RiskTrace.Core.Common;
using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Sessions;

public interface IGetSessionDetailUseCase
{
    Task<ApiResponse<SessionResponse>> ExecuteAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
