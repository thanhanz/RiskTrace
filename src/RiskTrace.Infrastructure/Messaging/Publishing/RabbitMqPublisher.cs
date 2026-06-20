using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RiskTrace.Domain.Events;
using RiskTrace.Domain.Messaging;
using RiskTrace.Infrastructure.Messaging.Connection;
using RiskTrace.UseCases.Ports.Messaging;

namespace RiskTrace.Infrastructure.Messaging.Publishing;

public sealed class RabbitMqPublisher(
    RabbitMqConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqPublisher> logger) : IMessageQueueService, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly SemaphoreSlim _channelLock = new(1, 1);
    private IChannel? _channel;

    public async Task PublishAsync<T>(
        string routingKey,
        T message,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var channel = await GetChannelAsync(cancellationToken);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message, JsonOptions));

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = typeof(T).Name,
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = message is EventBase evt ? evt.CorrelationId : null,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            exchange: options.Value.ExchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Published RabbitMQ message {EventType} with routing key {RoutingKey}.",
            typeof(T).Name,
            routingKey);
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

            var channelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true
            );

            _channel = await connection.CreateChannelAsync(options: channelOptions, cancellationToken: cancellationToken);
         
            await DeclareTopologyAsync(_channel, cancellationToken);

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
