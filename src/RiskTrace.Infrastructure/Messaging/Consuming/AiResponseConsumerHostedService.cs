using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RiskTrace.Domain.Events;
using RiskTrace.UseCases.Interfaces.ReviewResults;
using RiskTrace.UseCases.Ports.Messaging;

namespace RiskTrace.Infrastructure.Messaging.Consuming;

public sealed class AiResponseConsumerHostedService(
    IServiceScopeFactory scopeFactory,
    IMessageQueueConsumer messageQueueConsumer,
    IOptions<RabbitMqOptions> options,
    ILogger<AiResponseConsumerHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await messageQueueConsumer.ExecuteAsync<TemporaryAiResponseEvent>(
                    options.Value.AiResponseQueue,
                    HandleMessageAsync,
                    stoppingToken);

                logger.LogInformation(
                    "Backend AI response consumer registered for queue {QueueName}.",
                    options.Value.AiResponseQueue);

                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Backend AI response consumer is stopping.");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Backend AI response consumer failed to start for queue {QueueName}. Retrying in {RetryDelaySeconds} seconds.",
                    options.Value.AiResponseQueue,
                    RetryDelay.TotalSeconds);

                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task HandleMessageAsync(
        TemporaryAiResponseEvent response,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<IHandleAiResponseUseCase>();

        await useCase.ExecuteAsync(response, cancellationToken);
    }
}
