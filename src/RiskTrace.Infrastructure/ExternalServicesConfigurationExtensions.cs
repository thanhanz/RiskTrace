using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RiskTrace.Infrastructure.AI;
using RiskTrace.Infrastructure.Storage;
using RiskTrace.UseCases.Ports.AI;

namespace RiskTrace.Infrastructure;

internal static class ExternalServicesConfigurationExtensions
{
    public static IServiceCollection AddExternalServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ILegalAiClient, LegalAiHttpClient>();
        services.AddR2Storage(configuration);

        return services;
    }
}
