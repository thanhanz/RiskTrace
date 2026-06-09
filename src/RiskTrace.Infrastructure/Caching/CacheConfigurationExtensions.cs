using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RiskTrace.Infrastructure.Caching;

internal static class CacheConfigurationExtensions
{
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' was not found.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        return services;
    }
}
