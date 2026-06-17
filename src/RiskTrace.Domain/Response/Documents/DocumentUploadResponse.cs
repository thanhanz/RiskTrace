using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Response.Documents;

public sealed record DocumentUploadResponse(
    Guid DocumentId,
    FileUploadType UploadType,
    string ObjectKey,
    string? UploadUrl,
    string? UploadId,
    long? PartSizeBytes,
    IReadOnlyList<DocumentUploadPartResponse> Parts,
    DateTime ExpiresAt);

public sealed record DocumentUploadPartResponse(
    int PartNumber,
    string UploadUrl);
