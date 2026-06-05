using Microsoft.Extensions.DependencyInjection;
using RiskTrace.UseCases.Interfaces.Auth;
using RiskTrace.UseCases.UseCases.Auth;

namespace RiskTrace.UseCases;

public static class DependencyInjection
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<ILogoutUseCase, LogoutUseCase>();
        services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
        services.AddScoped<IRegisterUseCase, RegisterUseCase>();

        return services;
    }
}
