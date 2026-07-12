using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Configurations;

public class AccountConfiguration :
    IEntityTypeConfiguration<Account>
{
    public void Configure(
        EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.OpeningBalance)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.Color)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Icon)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.Name
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.UserId,
            x.IsActive
        });
    }
}