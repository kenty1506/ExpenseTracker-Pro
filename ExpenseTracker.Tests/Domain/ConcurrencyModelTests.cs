using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Tests.Domain;

public sealed class ConcurrencyModelTests
{
    public static IEnumerable<object[]> FinancialEntityTypes()
    {
        yield return [typeof(Account)];
        yield return [typeof(Budget)];
        yield return [typeof(Category)];
        yield return [typeof(FinancialGoal)];
        yield return [typeof(GoalContribution)];
        yield return [typeof(Notification)];
        yield return [typeof(RecurringTransaction)];
        yield return [typeof(global::Transaction)];
        yield return [typeof(Transfer)];
    }

    [Theory]
    [MemberData(nameof(FinancialEntityTypes))]
    public void FinancialEntities_UseTimestampConcurrencyToken(Type type)
    {
        Assert.True(type.IsSubclassOf(typeof(BaseEntity)));

        var rowVersion = typeof(BaseEntity).GetProperty(
            nameof(BaseEntity.RowVersion));

        Assert.NotNull(rowVersion);
        Assert.NotNull(Attribute.GetCustomAttribute(
            rowVersion,
            typeof(TimestampAttribute)));
    }
}
