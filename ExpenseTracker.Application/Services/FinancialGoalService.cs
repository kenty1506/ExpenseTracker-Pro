using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.FinancialGoals;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class FinancialGoalService : IFinancialGoalService
{
    private readonly IFinancialGoalRepository _financialGoalRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICategoryRepository _categoryRepository;

    public FinancialGoalService(
        IFinancialGoalRepository financialGoalRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _financialGoalRepository =
            financialGoalRepository;

        _accountRepository =
            accountRepository;

        _categoryRepository =
            categoryRepository;

        _currentUserService =
            currentUserService;
    }

    public Task<IEnumerable<FinancialGoalDto>> GetAllAsync()
    {
        return GetAllForUserAsync(
            _currentUserService.UserId);
    }

    public async Task<IEnumerable<FinancialGoalDto>>
        GetAllForUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "A valid user ID is required.",
                nameof(userId));
        }

        var goals =
            await _financialGoalRepository.GetAllAsync(
                userId);

        return goals.Select(MapToDto);
    }

    public async Task<FinancialGoalDto?> GetByIdAsync(int id)
    {
        var goal = await _financialGoalRepository.GetByIdAsync(id, _currentUserService.UserId);
        return goal == null ? null : MapToDto(goal);
    }


    public async Task<FinancialGoalDto> CreateAsync(CreateFinancialGoalDto dto)
    {
        var userId = _currentUserService.UserId;
        ValidateDates(dto.StartDate, dto.TargetDate);

        if (dto.StartingAmount < 0)
        {
            throw new ArgumentException("Starting amount cannot be negative.");
        }

        var name = dto.Name.Trim();
        var duplicate = await _financialGoalRepository.GetByNameAsync(name, userId);

        if (duplicate != null)
        {
            throw new ArgumentException($"A financial goal named '{name}' already exists.");
        }

        await ValidateAccountAsync(dto.AccountId, userId);
        var status = dto.StartingAmount >= dto.TargetAmount ? FinancialGoalStatus.Completed : FinancialGoalStatus.Active;

        var goal = new FinancialGoal
        {
            UserId = userId,
            Name = name,
            TargetAmount = dto.TargetAmount,
            StartingAmount = dto.StartingAmount,
            StartDate = dto.StartDate,
            TargetDate = dto.TargetDate,
            AccountId = dto.AccountId,
            Status = status,
            Color = dto.Color.Trim(),
            Icon = dto.Icon.Trim(),
            Notes = dto.Notes.Trim(),
            IsActive = true
        };

        var created = await _financialGoalRepository.CreateAsync(goal);
        var saved = await _financialGoalRepository.GetByIdAsync(created.Id, userId);

        if (saved == null)
        {
            throw new InvalidOperationException("The financial goal was created but could not be reloaded.");
        }
        return MapToDto(saved);
    }

    public async Task<FinancialGoalDto?> UpdateAsync(int id, UpdateFinancialGoalDto dto)
    {
        var userId = _currentUserService.UserId;
        var goal = await _financialGoalRepository.GetByIdAsync(id, userId);

        if (goal == null)
            return null;

        ValidateGoalStatus(dto.Status);
        ValidateDates(goal.StartDate, dto.TargetDate);

        if (dto.StartingAmount < 0)
        {
            throw new ArgumentException("Starting amount cannot be negative.");
        }

        var name = dto.Name.Trim();
        var duplicate = await _financialGoalRepository.GetByNameAsync(name, userId);

        if (duplicate != null && duplicate.Id != id)
        {
            throw new ArgumentException($"A financial goal named '{name}' already exists.");
        }

        await ValidateAccountAsync(dto.AccountId, userId);

        var savedAmount = dto.StartingAmount + goal.Contributions.Sum(contribution => contribution.Amount);

        goal.Name = name;
        goal.TargetAmount = dto.TargetAmount;
        goal.StartingAmount = dto.StartingAmount;
        goal.TargetDate = dto.TargetDate;
        goal.AccountId = dto.AccountId;
        goal.Color = dto.Color.Trim();
        goal.Icon = dto.Icon.Trim();
        goal.Notes = dto.Notes.Trim();

        goal.Status = savedAmount >= dto.TargetAmount ? FinancialGoalStatus.Completed : dto.Status;

        goal.UpdatedAt = DateTime.UtcNow;

        var updated = await _financialGoalRepository.UpdateAsync(goal);

        if (!updated)
            return null;

        var saved = await _financialGoalRepository.GetByIdAsync(id, userId);

        return saved == null
            ? null
            : MapToDto(saved);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _financialGoalRepository.DeleteAsync(id, _currentUserService.UserId);
    }


    public async Task<GoalContributionDto?> AddContributionAsync(
        int financialGoalId,
        AddGoalContributionDto dto)
    {
        var userId = _currentUserService.UserId;

        var goal =
            await _financialGoalRepository.GetByIdAsync(
                financialGoalId,
                userId);

        if (goal == null)
            return null;

        if (goal.Status == FinancialGoalStatus.Cancelled ||
            goal.Status == FinancialGoalStatus.Paused)
        {
            throw new ArgumentException(
                "Contributions cannot be added to a paused or cancelled goal.");
        }

        if (!dto.AccountId.HasValue)
        {
            throw new ArgumentException(
                "An account is required for a manual contribution.");
        }

        if (dto.Amount <= 0)
        {
            throw new ArgumentException(
                "The contribution amount must be greater than zero.");
        }

        await ValidateAccountAsync(
            dto.AccountId,
            userId);

        if (dto.ContributionDate.Date < goal.StartDate.Date)
        {
            throw new ArgumentException(
                "Contribution date cannot be earlier than the goal start date.");
        }

        var category =
            await GetFinancialGoalFundingCategoryAsync(
                userId);

        var transaction = new Transaction
        {
            UserId = userId,
            Type = TransactionType.Income,
            CategoryId = category.Id,
            AccountId = dto.AccountId,
            Amount = dto.Amount,
            Date = dto.ContributionDate,
            Notes =
                $"Financial goal contribution for '{goal.Name}': " +
                dto.Notes.Trim(),
            IsActive = true
        };

        var contribution = new GoalContribution
        {
            UserId = userId,
            FinancialGoalId = financialGoalId,
            AccountId = dto.AccountId,
            Amount = dto.Amount,
            ContributionDate = dto.ContributionDate,
            Notes = dto.Notes.Trim(),
            ContributionType = GoalContributionType.Manual,
            TransferId = null,
            TransactionId = null,
            IsActive = true
        };

        var created =
            await _financialGoalRepository
                .AddContributionWithTransactionAsync(
                    contribution,
                    transaction);

        await RecalculateGoalStatusAsync(
            financialGoalId,
            userId);

        var updatedGoal =
            await _financialGoalRepository.GetByIdAsync(
                financialGoalId,
                userId);

        if (updatedGoal == null)
        {
            throw new InvalidOperationException(
                "The contribution was created but the goal could not be reloaded.");
        }

        var savedContribution =
            updatedGoal.Contributions
                .FirstOrDefault(item =>
                    item.Id == created.Id);

        return savedContribution == null
            ? null
            : MapContributionToDto(savedContribution);
    }

    public async Task<GoalContributionDto?> AddAdjustmentAsync(
    int financialGoalId,
    AddGoalAdjustmentDto dto)
    {
        var userId =
            _currentUserService.UserId;

        var goal =
            await _financialGoalRepository.GetByIdAsync(
                financialGoalId,
                userId);

        if (goal == null)
            return null;

        if (goal.Status ==
                FinancialGoalStatus.Cancelled ||
            goal.Status ==
                FinancialGoalStatus.Paused)
        {
            throw new ArgumentException(
                "Adjustments cannot be added to a paused or cancelled goal.");
        }

        if (dto.Amount == 0)
        {
            throw new ArgumentException(
                "The adjustment amount cannot be zero.");
        }

        if (dto.AdjustmentDate.Date <
            goal.StartDate.Date)
        {
            throw new ArgumentException(
                "The adjustment date cannot be earlier than the goal start date.");
        }

        if (string.IsNullOrWhiteSpace(dto.Notes))
        {
            throw new ArgumentException(
                "Adjustment notes are required.");
        }

        var contribution =
            new GoalContribution
            {
                UserId =
                    userId,

                FinancialGoalId =
                    financialGoalId,

                AccountId =
                    goal.AccountId,

                Amount =
                    dto.Amount,

                ContributionDate =
                    dto.AdjustmentDate,

                Notes =
                    dto.Notes.Trim(),

                ContributionType =
                    GoalContributionType.Adjustment,

                TransferId =
                    null,

                IsActive =
                    true
            };

        var created =
            await _financialGoalRepository
                .AddContributionAsync(contribution);

        await RecalculateGoalStatusAsync(
            financialGoalId,
            userId);

        var updatedGoal =
            await _financialGoalRepository.GetByIdAsync(
                financialGoalId,
                userId);

        var savedContribution =
            updatedGoal?.Contributions
                .FirstOrDefault(item =>
                    item.Id == created.Id);

        return savedContribution == null
            ? null
            : MapContributionToDto(
                savedContribution);
    }


    public async Task<GoalContributionDto?> AddInterestAsync(
        int financialGoalId,
        AddGoalInterestDto dto)
    {
        var userId = _currentUserService.UserId;

        var goal =
            await _financialGoalRepository.GetByIdAsync(
                financialGoalId,
                userId);

        if (goal == null)
            return null;

        if (goal.Status == FinancialGoalStatus.Cancelled ||
            goal.Status == FinancialGoalStatus.Paused)
        {
            throw new ArgumentException(
                "Interest cannot be added to a paused or cancelled goal.");
        }

        if (!goal.AccountId.HasValue)
        {
            throw new ArgumentException(
                "The financial goal must be linked to an account before interest can be recorded.");
        }

        if (dto.Amount <= 0)
        {
            throw new ArgumentException(
                "The interest amount must be greater than zero.");
        }

        if (dto.InterestDate.Date < goal.StartDate.Date)
        {
            throw new ArgumentException(
                "The interest date cannot be earlier than the goal start date.");
        }

        await ValidateAccountAsync(
            goal.AccountId,
            userId);

        var notes =
            string.IsNullOrWhiteSpace(dto.Notes)
                ? "Interest earned"
                : dto.Notes.Trim();

        var category =
            await GetFinancialGoalFundingCategoryAsync(
                userId);

        var transaction = new Transaction
        {
            UserId = userId,
            Type = TransactionType.Income,
            CategoryId = category.Id,
            AccountId = goal.AccountId,
            Amount = dto.Amount,
            Date = dto.InterestDate,
            Notes =
                $"Interest earned for financial goal '{goal.Name}': {notes}",
            IsActive = true
        };

        var contribution = new GoalContribution
        {
            UserId = userId,
            FinancialGoalId = financialGoalId,
            AccountId = goal.AccountId,
            Amount = dto.Amount,
            ContributionDate = dto.InterestDate,
            Notes = notes,
            ContributionType = GoalContributionType.Interest,
            TransferId = null,
            TransactionId = null,
            IsActive = true
        };

        var created =
            await _financialGoalRepository
                .AddContributionWithTransactionAsync(
                    contribution,
                    transaction);

        await RecalculateGoalStatusAsync(
            financialGoalId,
            userId);

        var updatedGoal =
            await _financialGoalRepository.GetByIdAsync(
                financialGoalId,
                userId);

        var savedContribution =
            updatedGoal?.Contributions
                .FirstOrDefault(item =>
                    item.Id == created.Id);

        return savedContribution == null
            ? null
            : MapContributionToDto(savedContribution);
    }

    private async Task RecalculateGoalStatusAsync(
    int financialGoalId,
    string userId)
    {
        var goal =
            await _financialGoalRepository.GetByIdAsync(
                financialGoalId,
                userId);

        if (goal == null)
            return;

        if (goal.Status ==
                FinancialGoalStatus.Cancelled ||
            goal.Status ==
                FinancialGoalStatus.Paused)
        {
            return;
        }

        var savedAmount =
            goal.StartingAmount +
            goal.Contributions
                .Where(contribution =>
                    contribution.IsActive)
                .Sum(contribution =>
                    contribution.Amount);

        var expectedStatus =
            savedAmount >= goal.TargetAmount
                ? FinancialGoalStatus.Completed
                : FinancialGoalStatus.Active;

        if (goal.Status == expectedStatus)
            return;

        goal.Status =
            expectedStatus;

        goal.UpdatedAt =
            DateTime.UtcNow;

        await _financialGoalRepository.UpdateAsync(
            goal);
    }


    public async Task<bool> DeleteContributionAsync(
        int financialGoalId,
        int contributionId)
    {
        var userId = _currentUserService.UserId;

        var goal =
            await _financialGoalRepository.GetByIdAsync(
                financialGoalId,
                userId);

        if (goal == null)
            return false;

        var contribution =
            await _financialGoalRepository.GetContributionByIdAsync(
                contributionId,
                financialGoalId,
                userId);

        if (contribution == null)
            return false;

        if (contribution.TransferId.HasValue)
        {
            throw new ArgumentException(
                "Transfer-generated contributions cannot be deleted directly. " +
                "Update or delete the originating transfer instead.");
        }

        var deleted =
            await _financialGoalRepository
                .DeleteContributionWithTransactionAsync(
                    contributionId,
                    financialGoalId,
                    userId);

        if (!deleted)
            return false;

        await RecalculateGoalStatusAsync(
            financialGoalId,
            userId);

        return true;
    }

    private async Task ValidateAccountAsync(int? accountId, string userId)
    {
        if (!accountId.HasValue)
            return;

        var account = await _accountRepository.GetByIdAsync(accountId.Value, userId);

        if (account == null)
        {
            throw new ArgumentException($"Account with ID {accountId.Value} does not exist.");
        }

        if (!account.IsActive)
        {
            throw new ArgumentException("The selected account is inactive.");
        }
    }

    private static void ValidateDates(DateTime startDate, DateTime? targetDate)
    {
        if (targetDate.HasValue && targetDate.Value.Date < startDate.Date)
        {
            throw new ArgumentException("Target date cannot be earlier than the start date.");
        }
    }

    private static void ValidateGoalStatus(FinancialGoalStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentException("Please provide a valid financial goal status.");
        }
    }

    private static FinancialGoalDto MapToDto(FinancialGoal goal)
    {
        var savedAmount =
            goal.StartingAmount +
            goal.Contributions.Sum(contribution => contribution.Amount);

        var remainingAmount = Math.Max(goal.TargetAmount - savedAmount, 0);

        var percentageCompleted =
            goal.TargetAmount <= 0 ? 0 : Math.Round(savedAmount / goal.TargetAmount * 100, 2);

        var daysRemaining =
            goal.TargetDate.HasValue ? (goal.TargetDate.Value.Date - DateTime.UtcNow.Date).Days : 0;

        var isCompleted = savedAmount >= goal.TargetAmount ||
            goal.Status == FinancialGoalStatus.Completed;

        var isOverdue =
            goal.TargetDate.HasValue &&
            goal.TargetDate.Value.Date < DateTime.UtcNow.Date &&
            !isCompleted;

        return new FinancialGoalDto
        {
            Id = goal.Id,
            Name = goal.Name,
            TargetAmount = goal.TargetAmount,
            StartingAmount = goal.StartingAmount,
            SavedAmount = savedAmount,
            RemainingAmount = remainingAmount,
            PercentageCompleted = percentageCompleted,
            StartDate = goal.StartDate,
            TargetDate = goal.TargetDate,
            DaysRemaining = daysRemaining,
            Status = goal.Status,
            IsCompleted = isCompleted,
            IsOverdue = isOverdue,
            Color = goal.Color,
            Icon = goal.Icon,
            Notes = goal.Notes,
            AccountId = goal.AccountId,
            Account = goal.Account?.Name ?? string.Empty,
            Contributions = goal.Contributions
                    .OrderByDescending(contribution => contribution.ContributionDate)
                    .ThenByDescending(contribution => contribution.Id)
                    .Select(MapContributionToDto)
                    .ToList()
        };
    }

    private static GoalContributionDto MapContributionToDto(GoalContribution contribution)
    {
        return new GoalContributionDto
        {
            Id = contribution.Id,
            Amount = contribution.Amount,
            ContributionDate = contribution.ContributionDate,
            Notes = contribution.Notes,
            AccountId = contribution.AccountId,
            Account = contribution.Account?.Name ?? string.Empty,
            ContributionType = contribution.ContributionType,
            TransferId = contribution.TransferId,
            TransactionId = contribution.TransactionId
        };
    }
    public async Task<FinancialGoalsSummaryDto> GetSummaryAsync()
    {
        var goals =
            (await _financialGoalRepository.GetAllAsync(
                _currentUserService.UserId))
            .ToList();

        var today = DateTime.UtcNow.Date;

        var totalTargetAmount =
            goals.Sum(goal => goal.TargetAmount);

        var totalSavedAmount =
            goals.Sum(goal =>
                goal.StartingAmount +
                goal.Contributions.Sum(
                    contribution =>
                        contribution.Amount));

        var totalRemainingAmount =
            goals.Sum(goal =>
            {
                var saved =
                    goal.StartingAmount +
                    goal.Contributions.Sum(
                        contribution =>
                            contribution.Amount);

                return Math.Max(
                    goal.TargetAmount - saved,
                    0);
            });

        var completedGoals =
            goals.Count(goal =>
            {
                var saved =
                    goal.StartingAmount +
                    goal.Contributions.Sum(
                        contribution =>
                            contribution.Amount);

                return goal.Status ==
                           FinancialGoalStatus.Completed ||
                       saved >= goal.TargetAmount;
            });

        var overdueGoals =
            goals.Count(goal =>
            {
                var saved =
                    goal.StartingAmount +
                    goal.Contributions.Sum(
                        contribution =>
                            contribution.Amount);

                var completed =
                    goal.Status ==
                        FinancialGoalStatus.Completed ||
                    saved >= goal.TargetAmount;

                return goal.TargetDate.HasValue &&
                       goal.TargetDate.Value.Date < today &&
                       !completed;
            });

        var overallPercentageCompleted =
            totalTargetAmount <= 0
                ? 0
                : Math.Round(
                    totalSavedAmount /
                    totalTargetAmount *
                    100,
                    2);

        return new FinancialGoalsSummaryDto
        {
            TotalGoals = goals.Count,

            ActiveGoals = goals.Count(goal =>
                goal.Status ==
                FinancialGoalStatus.Active),

            CompletedGoals = completedGoals,

            PausedGoals = goals.Count(goal =>
                goal.Status ==
                FinancialGoalStatus.Paused),

            CancelledGoals = goals.Count(goal =>
                goal.Status ==
                FinancialGoalStatus.Cancelled),

            OverdueGoals = overdueGoals,

            TotalTargetAmount = totalTargetAmount,

            TotalSavedAmount = totalSavedAmount,

            TotalRemainingAmount = totalRemainingAmount,

            OverallPercentageCompleted =
                overallPercentageCompleted
        };
    }
    public async Task<PagedResult<FinancialGoalDto>> GetPagedAsync(
    FinancialGoalQueryDto query)
    {
        var result =
            await _financialGoalRepository.GetPagedAsync(
                _currentUserService.UserId,
                query);

        return new PagedResult<FinancialGoalDto>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages,
            Items = result.Items
                .Select(MapToDto)
                .ToList()
        };
    }

    private async Task<Category>
    GetFinancialGoalFundingCategoryAsync(
        string userId)
    {
        const string categoryName =
            "Financial Goal Funding";

        var category =
            await _categoryRepository.GetByNameAsync(
                categoryName,
                userId);

        if (category != null)
            return category;

        var created =
            new Category
            {
                UserId = userId,
                Name = categoryName,
                Color = "#10B981",
                Icon = "savings",
                IsActive = true
            };

        return await _categoryRepository.CreateAsync(
            created);
    }

}