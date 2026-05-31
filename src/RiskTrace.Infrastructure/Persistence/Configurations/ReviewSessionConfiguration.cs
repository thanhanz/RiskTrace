using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskTrace.Domain.Entities;

namespace RiskTrace.Infrastructure.Persistence.Configurations;

internal sealed class ReviewSessionConfiguration : IEntityTypeConfiguration<ReviewSession>
{
    public void Configure(EntityTypeBuilder<ReviewSession> builder)
    {
        builder.ToTable("ReviewSessions");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnType("uuid");

        builder.Property(entity => entity.UserId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Title)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditColumns();
    }
}
