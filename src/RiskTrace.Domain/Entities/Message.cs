using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Entities;

public sealed class Message : BaseModel
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public MessageRole Role { get; set; }

    public string Content { get; set; } = string.Empty;
}
