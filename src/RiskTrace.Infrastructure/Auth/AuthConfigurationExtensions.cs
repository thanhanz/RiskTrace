using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces;
using RiskTrace.UseCases.Ports.Auth;

namespace RiskTrace.Infrastructure.Auth;

internal static class AuthConfigurationExtensions
{

    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddSingleton<ITokenBlackList, TokenBlackList>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        return services;
    }

    public static IServiceCollection AddJwtOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // App will fail to start if JwtOptions are not properly configured.
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.AccessTokenCookieName),
                "Jwt:AccessTokenCookieName must not be empty.")
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddJwtBearerAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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
                    ClockSkew = TimeSpan.Zero
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
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        await context.Response.WriteAsJsonAsync(
                            CommonErrors.Unauthorized("Unauthenticated"),
                            context.HttpContext.RequestAborted);
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;

                        await context.Response.WriteAsJsonAsync(
                            CommonErrors.Forbidden(),
                            context.HttpContext.RequestAborted);
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
