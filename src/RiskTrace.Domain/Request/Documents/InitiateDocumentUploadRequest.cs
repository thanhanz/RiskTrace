namespace RiskTrace.Domain.Request.Documents;

public sealed record InitiateDocumentUploadRequest(
    string FileName,
    string ContentType,
    long FileSize);
