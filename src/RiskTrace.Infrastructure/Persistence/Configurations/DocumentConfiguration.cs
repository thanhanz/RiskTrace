using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskTrace.Domain.Entities;

namespace RiskTrace.Infrastructure.Persistence.Configurations;

internal sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnType("uuid");

        builder.Property(entity => entity.SessionId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.FileName)
            .IsRequired();

        builder.Property(entity => entity.FilePath)
            .IsRequired();

        builder.Property(entity => entity.ContentType)
            .IsRequired();

        builder.Property(entity => entity.FileSize)
            .HasColumnType("bigint")
            .IsRequired();

        builder.HasOne<ReviewSession>()
            .WithMany()
            .HasForeignKey(entity => entity.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditColumns();
    }
}
