namespace RiskTrace.UseCases.Ports.Auth;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
