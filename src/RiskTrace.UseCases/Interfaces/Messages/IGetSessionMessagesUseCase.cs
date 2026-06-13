using RiskTrace.Core.Common;
using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Messages;

public interface IGetSessionMessagesUseCase
{
    Task<ApiResponse<PaginatedResult<MessageResponse>>> ExecuteAsync(
        Guid sessionId,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);
}
