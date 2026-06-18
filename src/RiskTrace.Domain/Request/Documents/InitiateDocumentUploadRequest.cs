namespace RiskTrace.Domain.Request.Documents;

public class InitiateDocumentUploadRequest
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }
}
