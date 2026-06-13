using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Response;

public sealed record MessageResponse(
    Guid Id,
    Guid SessionId,
    MessageRole Role,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
