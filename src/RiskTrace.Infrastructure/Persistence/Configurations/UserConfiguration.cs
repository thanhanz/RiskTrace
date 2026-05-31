using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskTrace.Domain.Entities;

namespace RiskTrace.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnType("uuid");

        builder.Property(entity => entity.Email)
            .IsRequired();

        builder.Property(entity => entity.PasswordHash)
            .IsRequired();

        builder.Property(entity => entity.FullName)
            .IsRequired();

        builder.ConfigureAuditColumns();
    }
}
