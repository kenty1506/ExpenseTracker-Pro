using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Configurations;

public class FinancialGoalConfiguration : IEntityTypeConfiguration<FinancialGoal>
{
    public void Configure( EntityTypeBuilder<FinancialGoal> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.TargetAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.StartingAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.Color)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Icon)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.FinancialGoals)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.Status
        });

        builder.HasIndex(x => new
        {
            x.UserId,
            x.TargetDate
        });

        builder.HasIndex(x => new
        {
            x.UserId,
            x.Name
        })
        .IsUnique();

        builder.HasIndex(goal => new
        {
            goal.UserId,
            goal.Status
        });

        builder.HasIndex(goal => new
        {
            goal.UserId,
            goal.AccountId
        });

        builder.HasIndex(goal => new
        {
            goal.UserId,
            goal.TargetDate
        });

        builder.HasIndex(goal => new
        {
            goal.UserId,
            goal.Name
        });
    }
}