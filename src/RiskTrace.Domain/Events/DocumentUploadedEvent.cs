namespace RiskTrace.Domain.Events;

public sealed record DocumentUploadedEvent : EventBase
{
    public required Guid DocumentId { get; init; }

    public required Guid SessionId { get; init; }

    public required Guid UserId { get; init; }

    public required string StoragePath { get; init; }

    public required string FileName { get; init; }
}
