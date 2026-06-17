using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Response.Documents;

public sealed record DocumentResponse(
    Guid Id,
    Guid SessionId,
    string FileName,
    string FilePath,
    string ContentType,
    long FileSize,
    DocumentStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
