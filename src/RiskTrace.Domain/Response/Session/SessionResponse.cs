using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Response;

public sealed record SessionResponse(
    Guid Id,
    string Title,
    SessionStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
