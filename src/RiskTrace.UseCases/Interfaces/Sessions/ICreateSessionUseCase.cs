namespace RiskTrace.UseCases.Interfaces.Sessions;

public interface ICreateSessionUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
