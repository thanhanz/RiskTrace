using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiskTrace.Domain.Entities;

namespace RiskTrace.Infrastructure.Persistence.Configurations;

internal sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnType("uuid");

        builder.Property(entity => entity.SessionId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Content)
            .IsRequired();

        builder.HasOne<ReviewSession>()
            .WithMany()
            .HasForeignKey(entity => entity.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditColumns();
    }
}
