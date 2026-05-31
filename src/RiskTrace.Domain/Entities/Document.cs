using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Entities;

public sealed class Document : BaseModel
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DocumentStatus Status { get; set; }
}
