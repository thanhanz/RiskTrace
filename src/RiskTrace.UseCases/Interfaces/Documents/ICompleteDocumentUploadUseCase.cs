using RiskTrace.Core.Common;
using RiskTrace.Domain.Request.Documents;
using RiskTrace.Domain.Response.Documents;

namespace RiskTrace.UseCases.Interfaces.Documents;

public interface ICompleteDocumentUploadUseCase
{
    Task<ApiResponse<DocumentResponse>> ExecuteAsync(
        Guid sessionId,
        CompleteDocumentUploadRequest request,
        CancellationToken cancellationToken = default);
}
