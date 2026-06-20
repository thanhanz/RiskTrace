using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace RiskTrace.Infrastructure.Messaging.Connection;

public sealed class RabbitMqConnectionFactory(IOptions<RabbitMqOptions> options) : IAsyncDisposable
{
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            var opts = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = opts.Host,
                Port = opts.Port,
                VirtualHost = opts.VirtualHost,
                UserName = opts.Username,
                Password = opts.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _connectionLock.Dispose();

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
