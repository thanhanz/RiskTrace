using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RiskTrace.Core.Interfaces;
using RiskTrace.Infrastructure.Persistence.Repositories;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.Infrastructure.Persistence;

internal static class PersistenceConfigurationExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseProvider = configuration["Database:Provider"] ?? "PostgreSQL";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        if (!string.Equals(databaseProvider, "PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Database provider '{databaseProvider}' is not supported yet. Configure 'PostgreSQL' for now.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, postgreSqlOptions =>
                postgreSqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());

        return services;
    }

    public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IReviewSessionRepository, ReviewSessionRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IReviewResultRepository, ReviewResultRepository>();

        return services;
    }
}
