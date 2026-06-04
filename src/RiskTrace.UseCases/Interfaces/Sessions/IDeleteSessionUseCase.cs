namespace RiskTrace.UseCases.Interfaces.Sessions;

public interface IDeleteSessionUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
