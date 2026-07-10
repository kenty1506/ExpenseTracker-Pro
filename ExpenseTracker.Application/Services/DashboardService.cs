using ExpenseTracker.Application.DTOs.Dashboard;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var transactions =
            await _transactionRepository.GetAllForDashboardAsync(
                _currentUserService.UserId);

        var transactionList = transactions.ToList();

        var totalIncome = transactionList
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpense = transactionList
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        return new DashboardSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = totalIncome - totalExpense,
            TransactionCount = transactionList.Count()
        };
    }

    public async Task<IEnumerable<CategoryBreakdownDto>>
        GetCategoryBreakdownAsync()
    {
        var transactions =
            await _transactionRepository.GetAllAsync(
                _currentUserService.UserId);

        return transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category?.Name ?? "Uncategorized")
            .Select(group => new CategoryBreakdownDto
            {
                Category = group.Key,
                Amount = group.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();
    }
}