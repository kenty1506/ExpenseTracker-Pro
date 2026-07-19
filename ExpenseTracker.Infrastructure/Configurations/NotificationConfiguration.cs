using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Configurations;

public class NotificationConfiguration :
    IEntityTypeConfiguration<Notification>
{
    public void Configure(
        EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(100);

        builder.Property(x => x.ActionUrl)
            .HasMaxLength(500);

        builder.Property(x => x.UniqueKey)
            .HasMaxLength(300);

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.UserId,
            x.IsRead,
            x.OccurredAt
        });

        builder.HasIndex(x => new
        {
            x.UserId,
            x.Type
        });

        builder.HasIndex(x => new
        {
            x.UserId,
            x.UniqueKey
        })
        .IsUnique()
        .HasFilter("[UniqueKey] IS NOT NULL");

        builder.HasIndex(notification => new
        {
            notification.UserId,
            notification.IsRead,
            notification.CreatedAt
        });

        builder.HasIndex(notification => new
        {
            notification.UserId,
            notification.CreatedAt
        });
    }
}