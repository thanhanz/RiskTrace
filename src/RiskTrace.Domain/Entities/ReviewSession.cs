using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Entities;

public sealed class ReviewSession : BaseModel
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public SessionStatus Status { get; set; }
}
