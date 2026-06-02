using Microsoft.EntityFrameworkCore;
using RiskTrace.Domain.Entities;

namespace RiskTrace.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<ReviewSession> ReviewSessions => Set<ReviewSession>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<ReviewResult> ReviewResults => Set<ReviewResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
