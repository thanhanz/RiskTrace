namespace RiskTrace.Domain.Events;

public sealed record AiReviewCompletedEvent : EventBase
{
    public required Guid DocumentId { get; init; }

    public required Guid SessionId { get; init; }

    public required string Summary { get; init; }

    public required IReadOnlyList<string> RiskFlags { get; init; }
}
