using Microsoft.Extensions.DependencyInjection;
using RiskTrace.Infrastructure.AI;
using RiskTrace.Infrastructure.Storage;
using RiskTrace.UseCases.Ports.AI;
using RiskTrace.UseCases.Ports.Storage;

namespace RiskTrace.Infrastructure;

internal static class ExternalServicesConfigurationExtensions
{
    public static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        services.AddScoped<ILegalAiClient, LegalAiHttpClient>();
        services.AddScoped<IFileStorage, LocalFileStorage>();

        return services;
    }
}
