using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RiskTrace.Infrastructure.Messaging.Connection;
using RiskTrace.UseCases.Ports.Messaging;

namespace RiskTrace.Infrastructure.Messaging.Consuming;

public sealed class RabbitMqConsumer(
    RabbitMqConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConsumer> logger) : IMessageQueueConsumer, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly SemaphoreSlim _channelLock = new(1, 1);
    private IChannel? _channel;

    public async Task ExecuteAsync<T>(
        string queueName,
        Func<T, CancellationToken, Task> handleMessageAsync,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentNullException.ThrowIfNull(handleMessageAsync);

        var channel = await GetChannelAsync(cancellationToken);
        await DeclareTopologyAsync(channel, cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            await HandleDeliveryAsync(
                channel,
                queueName,
                args,
                handleMessageAsync,
                cancellationToken);
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Started RabbitMQ consumer for queue {QueueName} and message type {MessageType}.",
            queueName,
            typeof(T).Name);
    }

    private async Task HandleDeliveryAsync<T>(
        IChannel channel,
        string queueName,
        BasicDeliverEventArgs args,
        Func<T, CancellationToken, Task> handleMessageAsync,
        CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            var message = JsonSerializer.Deserialize<T>(args.Body.Span, JsonOptions);
            if (message is null)
            {
                logger.LogWarning(
                    "RabbitMQ message {DeliveryTag} from queue {QueueName} could not be deserialized as {MessageType}.",
                    args.DeliveryTag,
                    queueName,
                    typeof(T).Name);

                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: cancellationToken);
                return;
            }

            await handleMessageAsync(message, cancellationToken);

            await channel.BasicAckAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Consumed RabbitMQ message {DeliveryTag} from queue {QueueName}.",
                args.DeliveryTag,
                queueName);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(
                ex,
                "RabbitMQ message {DeliveryTag} from queue {QueueName} contains invalid JSON.",
                args.DeliveryTag,
                queueName);

            await channel.BasicNackAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "RabbitMQ message {DeliveryTag} from queue {QueueName} failed during handling.",
                args.DeliveryTag,
                queueName);

            await channel.BasicNackAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken: cancellationToken);
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _channelLock.WaitAsync(cancellationToken);

        try
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            var connection = await connectionFactory.GetConnectionAsync(cancellationToken);
            _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            return _channel;
        }
        finally
        {
            _channelLock.Release();
        }
    }

    private async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        var opts = options.Value;

        await channel.ExchangeDeclareAsync(
            exchange: opts.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: opts.DocumentQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: opts.AiResponseQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: opts.DocumentQueue,
            exchange: opts.ExchangeName,
            routingKey: "document.*",
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: opts.AiResponseQueue,
            exchange: opts.ExchangeName,
            routingKey: "ai.*",
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        _channelLock.Dispose();

        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }
    }
}
