using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RecurringTransaction)
            .WithMany(x => x.GeneratedTransactions)
            .HasForeignKey(x => x.RecurringTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.AccountId,
            x.Date
        });

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => new
            {
                x.UserId,
                x.RecurringTransactionId,
                x.Date
            })
            .IsUnique();

        builder.HasIndex(transaction => new
        {
            transaction.UserId,
            transaction.Date
        });

        builder.HasIndex(transaction => new
        {
            transaction.UserId,
            transaction.AccountId,
            transaction.Date
        });

        builder.HasIndex(transaction => new
        {
            transaction.UserId,
            transaction.CategoryId,
            transaction.Date
        });

        builder.HasIndex(transaction => new
        {
            transaction.UserId,
            transaction.Type,
            transaction.Date
        });
    }
}