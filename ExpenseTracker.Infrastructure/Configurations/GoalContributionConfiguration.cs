using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Configurations;

public class GoalContributionConfiguration :
    IEntityTypeConfiguration<GoalContribution>
{
    public void Configure(
        EntityTypeBuilder<GoalContribution> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.ContributionDate)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasOne(x => x.FinancialGoal)
            .WithMany(x => x.Contributions)
            .HasForeignKey(x => x.FinancialGoalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.GoalContributions)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.FinancialGoalId,
            x.ContributionDate
        });

        builder.Property(x => x.ContributionType)
        .IsRequired();

        builder.HasOne(x => x.Transfer)
            .WithMany(x => x.GoalContributions)
            .HasForeignKey(x => x.TransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.FinancialGoalId,
            x.TransferId,
            x.ContributionType
        })
            .IsUnique()
            .HasFilter("[TransferId] IS NOT NULL");

        builder.HasIndex(contribution => new
        {
            contribution.FinancialGoalId,
            contribution.TransferId,
            contribution.ContributionType
        })
            .IsUnique()
            .HasFilter("[TransferId] IS NOT NULL");

        builder.HasIndex(contribution => new
        {
            contribution.UserId,
            contribution.FinancialGoalId,
            contribution.ContributionDate
        });

        builder.HasIndex(contribution => new
        {
            contribution.TransferId,
            contribution.ContributionType
        })
            .HasFilter("[TransferId] IS NOT NULL");

        builder.HasOne(contribution =>
        contribution.Transaction)
            .WithMany(transaction =>
        transaction.GoalContributions)
            .HasForeignKey(contribution =>
        contribution.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(contribution =>
            contribution.TransactionId)
            .HasFilter("[TransactionId] IS NOT NULL");
    }
}