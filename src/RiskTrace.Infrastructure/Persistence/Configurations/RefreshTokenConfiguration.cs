using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskTrace.Domain.Entities;

namespace RiskTrace.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnType("uuid");

        builder.Property(entity => entity.UserId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.TokenHash)
            .IsRequired();

        builder.Property(entity => entity.ExpiresAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(entity => entity.RevokedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(entity => entity.ReplacedByTokenId)
            .HasColumnType("uuid");

        builder.HasIndex(entity => entity.TokenHash)
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditColumns();
    }
}
