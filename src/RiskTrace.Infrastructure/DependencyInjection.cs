using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Interfaces;
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
        //App will fail to start if JwtOptions are not properly configured, 
        // which is good because we don't want it to run without valid JWT settings.
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.AccessTokenCookieName),
                "Jwt:AccessTokenCookieName must not be empty.")
            .ValidateOnStart();

        var databaseProvider = configuration["Database:Provider"] ?? "PostgreSQL";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' was not found.");

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

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = configuration
                    .GetSection(JwtOptions.SectionName)
                    .Get<JwtOptions>()
                    ?? throw new InvalidOperationException("Jwt configuration was not found.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = AccessTokenReader.ReadToken(
                            context.HttpContext.Request,
                            jwtOptions.AccessTokenCookieName);

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var accessToken = context.Token;
                        if (string.IsNullOrWhiteSpace(accessToken))
                        {
                            return;
                        }

                        var revocationService = context.HttpContext.RequestServices
                            .GetRequiredService<IAccessTokenRevocationService>();

                        if (await revocationService.IsRevokedAsync(accessToken, context.HttpContext.RequestAborted))
                        {
                            context.Fail("Access token has been revoked.");
                        }
                    }
                };
            });

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());
        
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IReviewSessionRepository, ReviewSessionRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IReviewResultRepository, ReviewResultRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAccessTokenRevocationService, RedisAccessTokenRevocationService>();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        services.AddScoped<ILegalAiClient, LegalAiHttpClient>();
        services.AddScoped<IFileStorage, LocalFileStorage>();

        return services;
    }
}
