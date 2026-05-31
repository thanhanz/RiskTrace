using Microsoft.EntityFrameworkCore;
using RiskTrace.Infrastructure;
using RiskTrace.Infrastructure.Persistence;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //Will remove soon - API should not depend on Infrastructure
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}