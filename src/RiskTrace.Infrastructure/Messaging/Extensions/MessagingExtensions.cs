using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddScoped<IMessageQueueService, RabbitMqPublisher>();

        return services;
    }
}
