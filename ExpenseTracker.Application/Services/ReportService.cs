using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Application.Interfaces;

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
            throw new ArgumentException(
                "Please provide a valid report year.");
        }

        return await _reportRepository.GetMonthlyReportAsync(
            _currentUserService.UserId,
            year);
    }
}