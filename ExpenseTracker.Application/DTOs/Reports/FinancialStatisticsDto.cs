namespace ExpenseTracker.Application.DTOs.Reports;

public class FinancialStatisticsDto
{
    public int Year { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal Balance { get; set; }

    public decimal LargestIncome { get; set; }

    public decimal LargestExpense { get; set; }

    public decimal AverageIncome { get; set; }

    public decimal AverageExpense { get; set; }

    public decimal AverageTransactionAmount { get; set; }

    public int TransactionCount { get; set; }

    public int IncomeTransactionCount { get; set; }

    public int ExpenseTransactionCount { get; set; }

    public string TopExpenseCategory { get; set; } = string.Empty;

    public decimal TopExpenseCategoryAmount { get; set; }
}