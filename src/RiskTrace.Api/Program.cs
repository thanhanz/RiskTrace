using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using RiskTrace.Api.Middleware;
using RiskTrace.Infrastructure;
using RiskTrace.Infrastructure.Persistence;
using RiskTrace.UseCases;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RiskTrace API",
                Version = "v1"
            });
        });
        builder.Services.AddUseCases();

        //Will remove soon - API should not depend on Infrastructure
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }

        app.UseAuthentication();
        app.UseMiddleware<TokenBlacklistMiddleware>();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
