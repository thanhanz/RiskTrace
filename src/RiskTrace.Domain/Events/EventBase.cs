namespace RiskTrace.Domain.Events;

public abstract record EventBase
{
    protected EventBase()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAt { get; init; }

    public string? CorrelationId { get; init; }
}
