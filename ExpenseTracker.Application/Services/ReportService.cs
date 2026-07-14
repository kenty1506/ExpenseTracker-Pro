using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentUserService _currentUserService;

    public ReportService(
        IReportRepository reportRepository,
        ICurrentUserService currentUserService)
    {
        _reportRepository = reportRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<MonthlyReportDto>>
        GetMonthlyReportAsync(int year)
    {
        if (year < 2000 || year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException("Please provide a valid report year.");
        }
        return await _reportRepository.GetMonthlyReportAsync(_currentUserService.UserId,year);
    }

    public async Task<IEnumerable<TopCategoryDto>>GetTopCategoriesAsync(int year, int limit)
    {
        if (year < 2000 ||
            year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException("Please provide a valid report year.");
        }
        if (limit < 1 || limit > 20)
        {
            throw new ArgumentException("The limit must be between 1 and 20.");
        }

        return await _reportRepository.GetTopCategoriesAsync(_currentUserService.UserId,year,limit);
    }

    public async Task<IEnumerable<TrendReportDto>> GetTrendAsync(int year)
    {
        if (year < 2000 ||year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException("Please provide a valid report year.");
        }

        return await _reportRepository.GetTrendAsync(_currentUserService.UserId,year);
    }

    public async Task<IEnumerable<CashFlowReportDto>> GetCashFlowAsync(int year)
    {
        if (year < 2000 ||year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException("Please provide a valid report year.");
        }

        return await _reportRepository.GetCashFlowAsync(_currentUserService.UserId,year);
    }

    public async Task<IEnumerable<DailySpendingDto>>GetDailySpendingAsync(int year,int month)
    {
        if (year < 2000 ||year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException("Please provide a valid report year.");
        }
        if (month < 1 || month > 12)
        {
            throw new ArgumentException("The month must be between 1 and 12.");
        }
        return await _reportRepository.GetDailySpendingAsync(_currentUserService.UserId,year,month);
    }

    public async Task<IEnumerable<CalendarSpendingDto>>GetCalendarAsync(int year,int month)
    {
        if (year < 2000 ||year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException("Invalid year.");
        }
        if (month < 1 || month > 12)
        {
            throw new ArgumentException("Invalid month.");
        }
        return await _reportRepository.GetCalendarAsync(_currentUserService.UserId, year, month);
    }

    public async Task<IEnumerable<LargestTransactionDto>>GetLargestTransactionsAsync(int limit,TransactionType? type)
    {
        if (limit < 1 || limit > 50)
        {
            throw new ArgumentException("The limit must be between 1 and 50.");
        }
        if (type.HasValue && !Enum.IsDefined(type.Value))
        {
            throw new ArgumentException("The transaction type must be 1 for Income or 2 for Expense.");
        }
        return await _reportRepository.GetLargestTransactionsAsync(_currentUserService.UserId,limit,type);
    }

    public async Task<IEnumerable<CategoryComparisonDto>> GetCategoryComparisonAsync(int year)
    {
        if (year < 2000 || year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException("Please provide a valid report year.");
        }
        return await _reportRepository.GetCategoryComparisonAsync(_currentUserService.UserId,year);
    }

    public async Task<FinancialStatisticsDto>GetStatisticsAsync(int year)
    {
        if (year < 2000 ||year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException("Please provide a valid report year.");
        }
        return await _reportRepository.GetStatisticsAsync(_currentUserService.UserId,year);
    }

    public async Task<PagedResult<LargestTransactionDto>>
    GetLargestTransactionsPagedAsync(
        LargestTransactionQueryDto query)
    {
        if (query.Type.HasValue &&
            !Enum.IsDefined(query.Type.Value))
        {
            throw new ArgumentException(
                "The transaction type must be 1 for Income or 2 for Expense.");
        }

        if (query.MinAmount.HasValue &&
            query.MinAmount.Value < 0)
        {
            throw new ArgumentException(
                "Minimum amount cannot be negative.");
        }

        if (query.MaxAmount.HasValue &&
            query.MaxAmount.Value < 0)
        {
            throw new ArgumentException(
                "Maximum amount cannot be negative.");
        }

        if (query.MinAmount.HasValue &&
            query.MaxAmount.HasValue &&
            query.MinAmount.Value > query.MaxAmount.Value)
        {
            throw new ArgumentException(
                "Minimum amount cannot be greater than maximum amount.");
        }

        if (query.FromDate.HasValue &&
            query.ToDate.HasValue &&
            query.FromDate.Value.Date >
            query.ToDate.Value.Date)
        {
            throw new ArgumentException(
                "From date cannot be later than to date.");
        }

        return await _reportRepository
            .GetLargestTransactionsPagedAsync(
                _currentUserService.UserId,
                query);
    }
}