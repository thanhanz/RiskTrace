namespace RiskTrace.UseCases.Ports.Messaging;

public interface IMessageQueueService
{
    Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
        where T : class;
}
