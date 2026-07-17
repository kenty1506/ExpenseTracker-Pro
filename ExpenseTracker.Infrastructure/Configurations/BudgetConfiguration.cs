using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Configurations;

public class BudgetConfiguration :
    IEntityTypeConfiguration<Budget>
{
    public void Configure(
        EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.CategoryId,
            x.Year,
            x.Month
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.UserId,
            x.Year,
            x.Month
        });

        builder.HasIndex(budget => new
        {
            budget.UserId,
            budget.Year,
            budget.Month
        });

        builder.HasIndex(budget => new
        {
            budget.UserId,
            budget.CategoryId,
            budget.Year,
            budget.Month
        })
        .IsUnique();
    }
}