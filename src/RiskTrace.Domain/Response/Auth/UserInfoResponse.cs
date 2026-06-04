using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Response;

public sealed record UserInfoResponse(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role);
