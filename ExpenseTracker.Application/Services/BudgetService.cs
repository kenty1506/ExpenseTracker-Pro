using ExpenseTracker.Application.DTOs.Budgets;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IRecurringTransactionRepository _recurringTransactionRepository;

    public BudgetService(
        IBudgetRepository budgetRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        ITransactionRepository transactionRepository,
        IRecurringTransactionRepository recurringTransactionRepository)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _transactionRepository = transactionRepository;
        _recurringTransactionRepository = recurringTransactionRepository;
    }

    public async Task<IEnumerable<BudgetDto>> GetAllAsync()
    {
        var budgets = await _budgetRepository.GetAllAsync(_currentUserService.UserId);
        return budgets.Select(MapToDto);
    }

    public async Task<BudgetDto?> GetByIdAsync(int id)
    {
        var budget = await _budgetRepository.GetByIdAsync(id,_currentUserService.UserId);

        return budget == null ? null: MapToDto(budget);
    }

    public async Task<BudgetDto> CreateAsync(CreateBudgetDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId,_currentUserService.UserId);

        if (category == null)
        {
            throw new ArgumentException($"Category with ID {dto.CategoryId} does not exist.");
        }

        var existingBudgets =await _budgetRepository.GetAllAsync(_currentUserService.UserId);

        var duplicate = existingBudgets.Any(x =>
            x.CategoryId == dto.CategoryId &&
            x.Year == dto.Year &&
            x.Month == dto.Month);

        if (duplicate)
        {
            throw new ArgumentException("A budget already exists for this category and month.");
        }

        var budget = new Budget
        {
            UserId = _currentUserService.UserId,
            CategoryId = dto.CategoryId,
            Year = dto.Year,
            Month = dto.Month,
            Amount = dto.Amount
        };

        var created =await _budgetRepository.CreateAsync(budget);

        var savedBudget =await _budgetRepository.GetByIdAsync(created.Id,_currentUserService.UserId);

        if (savedBudget == null)
        {
            throw new InvalidOperationException("The budget was created but could not be reloaded.");
        }
        return MapToDto(savedBudget);
    }

    public async Task<BudgetDto?> UpdateAsync(int id,UpdateBudgetDto dto)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, _currentUserService.UserId);

        if (budget == null)
            return null;

        budget.Amount = dto.Amount;
        budget.UpdatedAt = DateTime.UtcNow;

        var updated =await _budgetRepository.UpdateAsync(budget);

        if (!updated)
            return null;

        var savedBudget =await _budgetRepository.GetByIdAsync(id,_currentUserService.UserId);

        return savedBudget == null
            ? null
            : MapToDto(savedBudget);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _budgetRepository.DeleteAsync(id,_currentUserService.UserId);
    }

    private static BudgetDto MapToDto(Budget budget)
    {
        return new BudgetDto
        {
            Id = budget.Id,
            CategoryId = budget.CategoryId,
            Category =budget.Category?.Name ?? string.Empty,
            Color =budget.Category?.Color ?? string.Empty,
            Icon =budget.Category?.Icon ?? string.Empty,
            Year = budget.Year,
            Month = budget.Month,
            Amount = budget.Amount
        };
    }
    public Task<IEnumerable<BudgetSummaryDto>> GetSummaryAsync(
    int year,
    int month)
    {
        return GetSummaryForUserAsync(
            _currentUserService.UserId,
            year,
            month);
    }

    private async Task<IEnumerable<BudgetSummaryDto>>
        GetSummaryForUserAsync(
            string userId,
            int year,
            int month)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "A valid user ID is required.",
                nameof(userId));
        }

        if (year < 2000 ||
            year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException(
                "Please provide a valid budget year.");
        }

        if (month < 1 || month > 12)
        {
            throw new ArgumentException(
                "The month must be between 1 and 12.");
        }

        var budgets =
            await _budgetRepository.GetByMonthAsync(
                userId,
                year,
                month);

        var monthStart = new DateTime(
            year,
            month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var nextMonthStart =
            monthStart.AddMonths(1);

        var transactions =
            await _transactionRepository.GetAllAsync(
                userId);

        var monthlyExpenses = transactions
            .Where(transaction =>
                transaction.Type ==
                    ExpenseTracker.Domain.Enums
                        .TransactionType.Expense &&
                transaction.Date >= monthStart &&
                transaction.Date < nextMonthStart)
            .ToList();

        return budgets
            .Select(budget =>
            {
                var categoryTransactions =
                    monthlyExpenses
                        .Where(transaction =>
                            transaction.CategoryId ==
                            budget.CategoryId)
                        .ToList();

                var actual =
                    categoryTransactions.Sum(
                        transaction =>
                            transaction.Amount);

                var remaining =
                    budget.Amount - actual;

                var percentageUsed =
                    budget.Amount <= 0
                        ? 0
                        : Math.Round(
                            actual /
                            budget.Amount *
                            100,
                            2);

                return new BudgetSummaryDto
                {
                    BudgetId = budget.Id,
                    CategoryId = budget.CategoryId,
                    Category =
                        budget.Category?.Name ??
                        string.Empty,
                    Color =
                        budget.Category?.Color ??
                        string.Empty,
                    Icon =
                        budget.Category?.Icon ??
                        string.Empty,
                    Budget = budget.Amount,
                    Actual = actual,
                    Remaining = remaining,
                    PercentageUsed =
                        percentageUsed,
                    IsOverBudget =
                        actual > budget.Amount,
                    TransactionCount =
                        categoryTransactions.Count
                };
            })
            .OrderByDescending(summary =>
                summary.PercentageUsed)
            .ToList();
    }

    public Task<IEnumerable<BudgetAlertDto>> GetAlertsAsync(
    int year,
    int month)
    {
        return GetAlertsForUserAsync(
            _currentUserService.UserId,
            year,
            month);
    }

    public async Task<IEnumerable<BudgetAlertDto>>
        GetAlertsForUserAsync(
            string userId,
            int year,
            int month)
    {
        var summaries =
            await GetSummaryForUserAsync(
                userId,
                year,
                month);

        return summaries
            .Select(summary =>
            {
                var alertLevel =
                    summary.PercentageUsed switch
                    {
                        >= 100 => "Over Budget",
                        >= 80 => "Warning",
                        _ => "Safe"
                    };

                var message =
                    alertLevel switch
                    {
                        "Over Budget" =>
                            $"You are over budget by " +
                            $"{Math.Abs(summary.Remaining):N2}.",

                        "Warning" =>
                            $"You have used " +
                            $"{summary.PercentageUsed:N2}% " +
                            $"of this budget.",

                        _ =>
                            $"You have " +
                            $"{summary.Remaining:N2} remaining."
                    };

                return new BudgetAlertDto
                {
                    BudgetId = summary.BudgetId,
                    Category = summary.Category,
                    Color = summary.Color,
                    Icon = summary.Icon,
                    Budget = summary.Budget,
                    Actual = summary.Actual,
                    Remaining = summary.Remaining,
                    PercentageUsed =
                        summary.PercentageUsed,
                    AlertLevel = alertLevel,
                    Message = message
                };
            })
            .OrderByDescending(alert =>
                alert.PercentageUsed)
            .ToList();
    }

    public async Task<IEnumerable<BudgetVsActualDto>>GetBudgetVsActualAsync(int year,int month)
    {
        var summaries = await GetSummaryAsync(year,month);
        return summaries
            .Select(summary =>
            {
                var variance =summary.Budget - summary.Actual;
                var variancePercentage =summary.Budget <= 0? 0: Math.Round(variance / summary.Budget * 100,2);
                var status = summary.Actual switch
                {
                    var actual when actual > summary.Budget =>
                        "Over Budget",

                    var actual when actual == summary.Budget =>
                        "On Budget",

                    _ =>
                        "Under Budget"
                };

                return new BudgetVsActualDto
                {
                    BudgetId = summary.BudgetId,
                    CategoryId = summary.CategoryId,
                    Category = summary.Category,
                    Color = summary.Color,
                    Icon = summary.Icon,
                    Budget = summary.Budget,
                    Actual = summary.Actual,
                    Variance = variance,
                    VariancePercentage = variancePercentage,
                    PercentageUsed = summary.PercentageUsed,
                    Status = status,
                    TransactionCount =
                        summary.TransactionCount
                };
            })
            .OrderByDescending(result =>
                result.PercentageUsed)
            .ToList();
    }

    public async Task<BudgetForecastDto> GetForecastAsync(
        int months,
        int historyMonths,
        decimal safetyBufferPercent)
    {
        if (months < 3 || months > 12)
        {
            throw new ArgumentException(
                "Forecast months must be between 3 and 12.");
        }

        if (historyMonths < 1 || historyMonths > 12)
        {
            throw new ArgumentException(
                "History months must be between 1 and 12.");
        }

        if (safetyBufferPercent < 0 || safetyBufferPercent > 50)
        {
            throw new ArgumentException(
                "The safety buffer must be between 0 and 50 percent.");
        }

        var userId = _currentUserService.UserId;

        // These repositories share a scoped DbContext, so load them sequentially.
        var budgets = await _budgetRepository.GetAllAsync(userId);
        var transactions = await _transactionRepository.GetAllAsync(userId);
        var recurringTransactions =
            await _recurringTransactionRepository.GetAllAsync(userId);

        return BudgetForecastCalculator.Calculate(
            budgets,
            transactions,
            recurringTransactions,
            DateTime.UtcNow,
            months,
            historyMonths,
            safetyBufferPercent);
    }
}
