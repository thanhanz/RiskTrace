namespace RiskTrace.UseCases.Interfaces.Messages;

public interface IGetSessionMessagesUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
