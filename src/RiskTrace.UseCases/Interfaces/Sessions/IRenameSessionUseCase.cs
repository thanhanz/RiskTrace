namespace RiskTrace.UseCases.Interfaces.Sessions;

public interface IRenameSessionUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
