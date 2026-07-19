using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Configurations;

public class RecurringTransactionConfiguration :
    IEntityTypeConfiguration<RecurringTransaction>
{
    public void Configure(
        EntityTypeBuilder<RecurringTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.DayOfMonth)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.NextRunDate)
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.RecurringTransactions)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.RecurringTransactions)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.AccountId
        });

        builder.HasIndex(x => new
        {
            x.UserId,
            x.IsActive,
            x.NextRunDate
        });

        builder.HasIndex(x => new
        {
            x.UserId,
            x.CategoryId
        });

        builder.HasIndex(recurring => new
        {
            recurring.UserId,
            recurring.IsActive,
            recurring.NextRunDate
        });

        builder.HasIndex(recurring => new
        {
            recurring.UserId,
            recurring.AccountId
        });
    }
}