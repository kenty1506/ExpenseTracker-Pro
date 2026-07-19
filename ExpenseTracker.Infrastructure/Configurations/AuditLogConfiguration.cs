using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Configurations;

public sealed class AuditLogConfiguration :
    IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(audit => audit.Id);

        builder.Property(audit => audit.UserId)
            .HasMaxLength(450);

        builder.Property(audit => audit.Method)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(audit => audit.Module)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(audit => audit.Operation)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(audit => audit.EntityId)
            .HasMaxLength(100);

        builder.Property(audit => audit.Route)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(audit => audit.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(audit => audit.TraceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(audit => audit.CreatedAtUtc);
        builder.HasIndex(audit => new
        {
            audit.UserId,
            audit.CreatedAtUtc
        });

        builder.HasIndex(audit => new
        {
            audit.UserId,
            audit.Module,
            audit.CreatedAtUtc
        });

        builder.HasIndex(audit => new
        {
            audit.UserId,
            audit.Module,
            audit.EntityId
        });
    }
}
