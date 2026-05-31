using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RiskTrace.Infrastructure.AI;
using RiskTrace.Infrastructure.Auth;
using RiskTrace.Infrastructure.Persistence;
using RiskTrace.Infrastructure.Persistence.Repositories;
using RiskTrace.Infrastructure.Storage;
using RiskTrace.UseCases.Ports.AI;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;
using RiskTrace.UseCases.Ports.Storage;

namespace RiskTrace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
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

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReviewSessionRepository, ReviewSessionRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IReviewResultRepository, ReviewResultRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        services.AddScoped<ILegalAiClient, LegalAiHttpClient>();
        services.AddScoped<IFileStorage, LocalFileStorage>();

        return services;
    }
}
