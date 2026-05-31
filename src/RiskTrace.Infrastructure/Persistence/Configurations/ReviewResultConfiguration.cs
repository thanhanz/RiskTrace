using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskTrace.Domain.Entities;

namespace RiskTrace.Infrastructure.Persistence.Configurations;

internal sealed class ReviewResultConfiguration : IEntityTypeConfiguration<ReviewResult>
{
    public void Configure(EntityTypeBuilder<ReviewResult> builder)
    {
        builder.ToTable("ReviewResults");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnType("uuid");

        builder.Property(entity => entity.SessionId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Summary)
            .IsRequired();

        builder.Property(entity => entity.ResultJson)
            .IsRequired();

        builder.HasOne<ReviewSession>()
            .WithMany()
            .HasForeignKey(entity => entity.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditColumns();
    }
}
