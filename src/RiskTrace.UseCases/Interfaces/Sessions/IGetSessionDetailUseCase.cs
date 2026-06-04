namespace RiskTrace.UseCases.Interfaces.Sessions;

public interface IGetSessionDetailUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
