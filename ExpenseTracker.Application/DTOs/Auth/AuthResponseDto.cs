namespace ExpenseTracker.Application.DTOs.Auth;

public class AuthResponseDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public DateTime? TokenExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore]
    public string? AuditUserId { get; set; }
}
