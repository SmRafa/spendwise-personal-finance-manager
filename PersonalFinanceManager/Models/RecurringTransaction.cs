using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Models;

public class RecurringTransaction
{
    public int Id { get; set; }
    public TransactionType Type { get; set; }

    [Range(1, 9_999_999_999)]
    public decimal Amount { get; set; }

    [Required, StringLength(250)]
    public string Note { get; set; } = string.Empty;

    [Display(Name = "Account")]
    [Range(1, int.MaxValue)]
    public int AccountId { get; set; }
    public Account? Account { get; set; }

    [Display(Name = "Category")]
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Display(Name = "First due date")]
    [DataType(DataType.Date)]
    public DateTime NextDueDate { get; set; } = DateTime.Today;

    public bool IsActive { get; set; } = true;
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
