using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RiskTrace.Infrastructure.Auth;
using RiskTrace.Infrastructure.Caching;
using RiskTrace.Infrastructure.Messaging.Extensions;
using RiskTrace.Infrastructure.Persistence;

namespace RiskTrace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddJwtOptions(configuration);
        services.AddPersistence(configuration);
        services.AddCaching(configuration);
        services.AddRabbitMq(configuration);
        services.AddJwtBearerAuthentication(configuration);
        services.AddInfrastructureRepositories();
        services.AddAuthServices();
        services.AddExternalServices(configuration);
        services.AddHttpContextAccessor();

        return services;
    }
}
