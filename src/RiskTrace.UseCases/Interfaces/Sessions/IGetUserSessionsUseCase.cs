namespace RiskTrace.UseCases.Interfaces.Sessions;

public interface IGetUserSessionsUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
