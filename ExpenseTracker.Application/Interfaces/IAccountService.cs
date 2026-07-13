using ExpenseTracker.Application.DTOs.Accounts;

namespace ExpenseTracker.Application.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllAsync();

    Task<AccountDetailsDto?> GetByIdAsync(int id);

    Task<AccountDto> CreateAsync(CreateAccountDto dto);

    Task<AccountDto?> UpdateAsync(int id,UpdateAccountDto dto);

    Task<bool> DeleteAsync(int id);

    Task<AccountSummaryDto> GetSummaryAsync();
}