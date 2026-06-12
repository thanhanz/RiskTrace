using RiskTrace.Core.Common;

namespace RiskTrace.UseCases.Interfaces.Sessions;

public interface IDeleteSessionUseCase
{
    Task<ApiResponse<object?>> ExecuteAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
