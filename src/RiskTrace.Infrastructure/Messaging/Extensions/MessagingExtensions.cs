using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RiskTrace.Infrastructure.Messaging.Consuming;
using RiskTrace.Infrastructure.Messaging.Connection;
using RiskTrace.Infrastructure.Messaging.Publishing;
using RiskTrace.UseCases.Ports.Messaging;

namespace RiskTrace.Infrastructure.Messaging.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<RabbitMqConnectionFactory>();
        // Scoped - need for per-request channel management in RabbitMqConsumer
        services.AddScoped<IMessageQueuePublisher, RabbitMqPublisher>();

        // Singleton - continue background consuming in the whole app lifecycle
        services.AddSingleton<IMessageQueueConsumer, RabbitMqConsumer>();
        services.AddHostedService<AiResponseConsumerHostedService>();

        return services;
    }
}
