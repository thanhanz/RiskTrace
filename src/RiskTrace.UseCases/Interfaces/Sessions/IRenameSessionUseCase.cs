using RiskTrace.Core.Common;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Sessions;

public interface IRenameSessionUseCase
{
    Task<ApiResponse<SessionResponse>> ExecuteAsync(
        Guid sessionId,
        RenameSessionRequest request,
        CancellationToken cancellationToken = default);
}
