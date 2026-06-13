using RiskTrace.Core.Common;
using RiskTrace.Domain.Enums;
using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Messages;

public interface ISendMessageUseCase
{
    Task<ApiResponse<MessageResponse>> ExecuteAsync(
        Guid sessionId,
        string content,
        MessageRole role,
        CancellationToken cancellationToken = default);
}
