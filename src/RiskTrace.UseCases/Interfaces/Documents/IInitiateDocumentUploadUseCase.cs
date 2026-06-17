using RiskTrace.Core.Common;
using RiskTrace.Domain.Request.Documents;
using RiskTrace.Domain.Response.Documents;

namespace RiskTrace.UseCases.Interfaces.Documents;

public interface IInitiateDocumentUploadUseCase
{
    Task<ApiResponse<DocumentUploadResponse>> ExecuteAsync(
        Guid sessionId,
        InitiateDocumentUploadRequest request,
        CancellationToken cancellationToken = default);
}
