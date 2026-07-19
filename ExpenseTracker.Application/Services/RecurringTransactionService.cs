using ExpenseTracker.Application.DTOs.RecurringTransactions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class RecurringTransactionService: IRecurringTransactionService
{
    private readonly IRecurringTransactionRepository
        _recurringTransactionRepository;

    private readonly ICategoryRepository
        _categoryRepository;

    private readonly ICurrentUserService
        _currentUserService;

    private readonly IAccountRepository _accountRepository;

    public RecurringTransactionService(
        IRecurringTransactionRepository recurringTransactionRepository,
        ICategoryRepository categoryRepository,
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService)
    {
        _recurringTransactionRepository = recurringTransactionRepository;
        _categoryRepository = categoryRepository;
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<RecurringTransactionDto>>
        GetAllAsync()

    {
        var recurringTransactions = await _recurringTransactionRepository.GetAllAsync(_currentUserService.UserId);

        return recurringTransactions.Select(MapToDto);
    }

    public async Task<RecurringTransactionDto?>GetByIdAsync(int id)
    {
        var recurringTransaction = await _recurringTransactionRepository.GetByIdAsync(id,_currentUserService.UserId);

        return recurringTransaction == null
            ? null
            : MapToDto(recurringTransaction);
    }

    public async Task<RecurringTransactionDto> CreateAsync(
    CreateRecurringTransactionDto dto)
    {
        var userId = _currentUserService.UserId;

        var category = await _categoryRepository.GetByIdAsync(
            dto.CategoryId,
            userId);

        if (category == null)
        {
            throw new ArgumentException(
                $"Category with ID {dto.CategoryId} does not exist.");
        }

        var account = await _accountRepository.GetByIdAsync(
            dto.AccountId,
            userId);

        if (account == null)
        {
            throw new ArgumentException(
                $"Account with ID {dto.AccountId} does not exist.");
        }

        if (!account.IsActive)
        {
            throw new ArgumentException(
                "The selected account is inactive.");
        }

        if (dto.EndDate.HasValue &&
            dto.EndDate.Value.Date < dto.StartDate.Date)
        {
            throw new ArgumentException(
                "End date cannot be earlier than start date.");
        }

        var nextRunDate = CalculateFirstRunDate(
            dto.StartDate,
            dto.DayOfMonth);

        var recurringTransaction = new RecurringTransaction
        {
            UserId = userId,
            Type = dto.Type,
            CategoryId = dto.CategoryId,
            AccountId = dto.AccountId,
            Amount = dto.Amount,
            Notes = dto.Notes,
            DayOfMonth = dto.DayOfMonth,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            NextRunDate = nextRunDate,
            IsActive = true
        };

        var created =
            await _recurringTransactionRepository.CreateAsync(
                recurringTransaction);

        var saved =
            await _recurringTransactionRepository.GetByIdAsync(
                created.Id,
                userId);

        if (saved == null)
        {
            throw new InvalidOperationException(
                "The recurring transaction was created but could not be reloaded.");
        }

        return MapToDto(saved);
    }

    public async Task<RecurringTransactionDto?> UpdateAsync(
    int id,
    UpdateRecurringTransactionDto dto)
    {
        var userId = _currentUserService.UserId;

        var recurringTransaction =
            await _recurringTransactionRepository.GetByIdAsync(
                id,
                userId);

        if (recurringTransaction == null)
            return null;

        var category =
            await _categoryRepository.GetByIdAsync(
                dto.CategoryId,
                userId);

        if (category == null)
        {
            throw new ArgumentException(
                $"Category with ID {dto.CategoryId} does not exist.");
        }

        var account =
            await _accountRepository.GetByIdAsync(
                dto.AccountId,
                userId);

        if (account == null)
        {
            throw new ArgumentException(
                $"Account with ID {dto.AccountId} does not exist.");
        }

        if (!account.IsActive)
        {
            throw new ArgumentException(
                "The selected account is inactive.");
        }

        if (dto.EndDate.HasValue &&
            dto.EndDate.Value.Date <
            recurringTransaction.StartDate.Date)
        {
            throw new ArgumentException(
                "End date cannot be earlier than start date.");
        }

        recurringTransaction.Type = dto.Type;
        recurringTransaction.CategoryId = dto.CategoryId;
        recurringTransaction.AccountId = dto.AccountId;
        recurringTransaction.Amount = dto.Amount;
        recurringTransaction.Notes = dto.Notes;
        recurringTransaction.DayOfMonth = dto.DayOfMonth;
        recurringTransaction.EndDate = dto.EndDate;
        recurringTransaction.IsActive = dto.IsActive;
        recurringTransaction.UpdatedAt = DateTime.UtcNow;

        if (recurringTransaction.LastRunDate == null)
        {
            recurringTransaction.NextRunDate =
                CalculateFirstRunDate(
                    recurringTransaction.StartDate,
                    dto.DayOfMonth);
        }
        else
        {
            recurringTransaction.NextRunDate =
                CalculateNextRunDate(
                    recurringTransaction.LastRunDate.Value,
                    dto.DayOfMonth);
        }

        var updated =
            await _recurringTransactionRepository.UpdateAsync(
                recurringTransaction);

        if (!updated)
            return null;

        var saved =
            await _recurringTransactionRepository.GetByIdAsync(
                id,
                userId);

        return saved == null
            ? null
            : MapToDto(saved);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _recurringTransactionRepository.DeleteAsync(id, _currentUserService.UserId);
    }

    private static DateTime CalculateFirstRunDate(DateTime startDate,int dayOfMonth)
    {
        var daysInMonth = DateTime.DaysInMonth(startDate.Year,startDate.Month);

        var validDay = Math.Min(dayOfMonth,daysInMonth);

        var candidate = new DateTime(startDate.Year,startDate.Month,validDay,0,0,0,DateTimeKind.Utc);

        if (candidate.Date < startDate.Date)
        {
            return CalculateNextRunDate(candidate,dayOfMonth);
        }

        return candidate;
    }

    private static DateTime CalculateNextRunDate(DateTime currentRunDate,int dayOfMonth)
    {
        var nextMonth = new DateTime(currentRunDate.Year,currentRunDate.Month,1,0,0,0,DateTimeKind.Utc).AddMonths(1);
        var daysInMonth = DateTime.DaysInMonth(nextMonth.Year,nextMonth.Month);
        var validDay = Math.Min(dayOfMonth,daysInMonth);
        return new DateTime(nextMonth.Year,nextMonth.Month,validDay,0,0,0,DateTimeKind.Utc);
    }

    private static RecurringTransactionDto MapToDto(
    RecurringTransaction recurringTransaction)
    {
        return new RecurringTransactionDto
        {
            Id = recurringTransaction.Id,
            Type = recurringTransaction.Type,

            CategoryId = recurringTransaction.CategoryId,
            Category = recurringTransaction.Category?.Name
                ?? string.Empty,
            Color = recurringTransaction.Category?.Color
                ?? string.Empty,
            Icon = recurringTransaction.Category?.Icon
                ?? string.Empty,

            AccountId = recurringTransaction.AccountId,
            Account = recurringTransaction.Account?.Name
                ?? string.Empty,

            Amount = recurringTransaction.Amount,
            Notes = recurringTransaction.Notes,
            DayOfMonth = recurringTransaction.DayOfMonth,
            StartDate = recurringTransaction.StartDate,
            EndDate = recurringTransaction.EndDate,
            NextRunDate = recurringTransaction.NextRunDate,
            LastRunDate = recurringTransaction.LastRunDate,
            IsActive = recurringTransaction.IsActive
        };
    }
    public Task<RecurringGenerationResultDto> GenerateDueAsync(
    DateTime? throughDate = null)
    {
        return GenerateDueForUserAsync(
            _currentUserService.UserId,
            throughDate);
    }

    public async Task<RecurringGenerationResultDto>
        GenerateDueForUserAsync(
            string userId,
            DateTime? throughDate = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "A valid user ID is required.",
                nameof(userId));
        }

        var processedThrough =
            (throughDate ?? DateTime.UtcNow).Date;

        var generatedItems =
            new List<GeneratedRecurringTransactionDto>();

        while (true)
        {
            var dueItems =
                (await _recurringTransactionRepository.GetDueAsync(
                    userId,
                    processedThrough))
                .ToList();

            if (dueItems.Count == 0)
                break;

            var processedAnyItem = false;

            foreach (var recurring in dueItems)
            {
                var occurrenceDate =
                    recurring.NextRunDate.Date;

                var nextRunDate =
                    CalculateNextRunDate(
                        occurrenceDate,
                        recurring.DayOfMonth);

                var generatedTransaction =
                    await _recurringTransactionRepository
                        .GenerateOccurrenceAsync(
                            recurring.Id,
                            userId,
                            occurrenceDate,
                            nextRunDate);

                /*
                 * The repository advances the schedule even when an existing
                 * generated occurrence is detected.
                 */
                processedAnyItem = true;

                if (generatedTransaction == null)
                    continue;

                generatedItems.Add(
                    new GeneratedRecurringTransactionDto
                    {
                        RecurringTransactionId =
                            recurring.Id,
                        TransactionId =
                            generatedTransaction.Id,
                        Category =
                            recurring.Category?.Name
                            ?? string.Empty,
                        Amount =
                            generatedTransaction.Amount,
                        OccurrenceDate =
                            occurrenceDate,
                        NextRunDate =
                            nextRunDate
                    });
            }

            // Defensive protection against an unexpected infinite loop.
            if (!processedAnyItem)
                break;
        }

        return new RecurringGenerationResultDto
        {
            ProcessedThrough = processedThrough,
            GeneratedCount = generatedItems.Count,
            Generated = generatedItems
        };
    }

    public Task<IEnumerable<UpcomingRecurringTransactionDto>>
    GetUpcomingAsync(int days)
    {
        return GetUpcomingForUserAsync(
            _currentUserService.UserId,
            days);
    }

    public async Task<IEnumerable<UpcomingRecurringTransactionDto>>
        GetUpcomingForUserAsync(
            string userId,
            int days)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "A valid user ID is required.",
                nameof(userId));
        }

        if (days < 1 || days > 365)
        {
            throw new ArgumentException(
                "The number of days must be between 1 and 365.");
        }

        var today = DateTime.UtcNow.Date;
        var throughDate = today.AddDays(days);

        var recurringTransactions =
            await _recurringTransactionRepository
                .GetUpcomingAsync(
                    userId,
                    today,
                    throughDate);

        return recurringTransactions
            .Select(recurring =>
            {
                var daysUntilDue =
                    (recurring.NextRunDate.Date - today).Days;

                return new UpcomingRecurringTransactionDto
                {
                    Id = recurring.Id,
                    Type = recurring.Type,

                    CategoryId = recurring.CategoryId,
                    Category =
                        recurring.Category?.Name
                        ?? string.Empty,
                    Color =
                        recurring.Category?.Color
                        ?? string.Empty,
                    Icon =
                        recurring.Category?.Icon
                        ?? string.Empty,

                    AccountId = recurring.AccountId,
                    Account =
                        recurring.Account?.Name
                        ?? string.Empty,

                    Amount = recurring.Amount,
                    Notes = recurring.Notes,
                    NextRunDate = recurring.NextRunDate,
                    DaysUntilDue = daysUntilDue,
                    IsDueToday = daysUntilDue == 0,
                    IsOverdue = daysUntilDue < 0
                };
            })
            .ToList();
    }
}