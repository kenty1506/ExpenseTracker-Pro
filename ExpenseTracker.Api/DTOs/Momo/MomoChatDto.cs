using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.DTOs.Momo;

public sealed class MomoChatRequest
{
    [Required]
    [StringLength(1200, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(12)]
    public List<MomoConversationMessage> History { get; set; } = [];

    [StringLength(160)]
    public string CurrentPath { get; set; } = "/dashboard";
}

public sealed class MomoConversationMessage
{
    [Required]
    [RegularExpression("^(user|assistant)$")]
    public string Role { get; set; } = string.Empty;

    [Required]
    [StringLength(1200, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;
}

public sealed class MomoChatResponse
{
    public string Message { get; set; } = string.Empty;

    public string? SuggestedRoute { get; set; }

    public string? SuggestedRouteLabel { get; set; }

    public List<string> SuggestedPrompts { get; set; } = [];

    public bool IsSmartResponse { get; set; }

    public DateTime GeneratedAt { get; set; }
}
