using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string? RefreshTokenHash { get; set; }

    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public DateTime? RefreshTokenCreatedAtUtc { get; set; }
}
