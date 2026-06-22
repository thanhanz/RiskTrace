namespace RiskTrace.UseCases.Ports.Messaging;

public interface IMessageQueueConsumer
{
    Task ExecuteAsync<T>(
        string queueName,
        Func<T, CancellationToken, Task> handleMessageAsync,
        CancellationToken cancellationToken = default)
        where T : class;
}
