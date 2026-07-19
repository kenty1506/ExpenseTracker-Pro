using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Transfers;

public class UpdateTransferDto
{
    public int FromAccountId { get; set; }

    public int ToAccountId { get; set; }

    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    public DateTime TransferDate { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}