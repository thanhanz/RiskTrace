using Microsoft.Extensions.DependencyInjection;
using RiskTrace.UseCases.Interfaces.Auth;
using RiskTrace.UseCases.Interfaces.Messages;
using RiskTrace.UseCases.Interfaces.Sessions;
using RiskTrace.UseCases.UseCases.Auth;
using RiskTrace.UseCases.UseCases.Messages;
using RiskTrace.UseCases.UseCases.Sessions;

namespace RiskTrace.UseCases;

public static class DependencyInjection
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        //Authentication
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<ILogoutUseCase, LogoutUseCase>();
        services.AddScoped<IMyInfoUseCase, MyInfoUseCase>();
        services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
        services.AddScoped<IRegisterUseCase, RegisterUseCase>();
        //Messages
        services.AddScoped<ISendMessageUseCase, SendMessageUseCase>();
        //Sessions
        services.AddScoped<ICreateSessionUseCase, CreateSessionUseCase>();
        services.AddScoped<IGetUserSessionsUseCase, GetUserSessionsUseCase>();
        services.AddScoped<IGetSessionDetailUseCase, GetSessionDetailUseCase>();
        services.AddScoped<IRenameSessionUseCase, RenameSessionUseCase>();
        services.AddScoped<IDeleteSessionUseCase, DeleteSessionUseCase>();

        return services;
    }
}
