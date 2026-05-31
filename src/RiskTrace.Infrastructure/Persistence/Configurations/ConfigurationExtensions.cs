using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskTrace.Domain.Entities;

namespace RiskTrace.Infrastructure.Persistence.Configurations;

internal static class ConfigurationExtensions
{
    public static void ConfigureAuditColumns<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseModel
    {
        builder.Property(entity => entity.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(entity => entity.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(entity => entity.IsActive)
            .IsRequired();
    }
}
