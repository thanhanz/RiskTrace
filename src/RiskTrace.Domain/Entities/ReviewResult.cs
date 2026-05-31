using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Entities;

public sealed class ReviewResult : BaseModel
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public RiskLevel OverallRiskLevel { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string ResultJson { get; set; } = string.Empty;
}
