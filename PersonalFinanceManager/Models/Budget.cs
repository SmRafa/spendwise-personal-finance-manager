using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Models;

public class Budget
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select an expense category.")]
    [Display(Name = "Expense category")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Range(1, 9_999_999_999)]
    [Display(Name = "Monthly limit")]
    public decimal Amount { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
}
