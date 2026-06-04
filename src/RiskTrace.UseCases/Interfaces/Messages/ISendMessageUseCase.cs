namespace RiskTrace.UseCases.Interfaces.Messages;

public interface ISendMessageUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
