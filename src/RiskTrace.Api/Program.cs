using Microsoft.EntityFrameworkCore;
using RiskTrace.Infrastructure;
using RiskTrace.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.MapGet("/", () => "Hello World!");

app.Run();
