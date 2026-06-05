namespace RiskTrace.UseCases.Ports.Auth;

public interface ICurrentUserProvider
{
    Guid? UserId { get; }

    string? Email { get; }
}
