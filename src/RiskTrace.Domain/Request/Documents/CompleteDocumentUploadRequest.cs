using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Request.Documents;

public sealed record CompleteDocumentUploadRequest(
    Guid DocumentId,
    FileUploadType UploadType,
    string ObjectKey,
    string FileName,
    string ContentType,
    long FileSize,
    string? UploadId,
    IReadOnlyList<CompletedDocumentUploadPartRequest>? Parts);

public sealed record CompletedDocumentUploadPartRequest(
    int PartNumber,
    string ETag);
