using RiskTrace.Domain.Enums;

namespace RiskTrace.Domain.Entities;

public sealed class User : BaseModel
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}
