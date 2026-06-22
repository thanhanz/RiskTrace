namespace RiskTrace.UseCases.Ports.Messaging;

public interface IMessageQueuePublisher
{
    Task ExecuteAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
        where T : class;
}
