using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Models;

public class FinanceTransaction
{
    public int Id { get; set; }

    [Display(Name = "Transaction type")]
    public TransactionType Type { get; set; }

    [Range(1, 9_999_999_999)]
    public decimal Amount { get; set; }

    [Display(Name = "Transaction date")]
    [DataType(DataType.Date)]
    public DateTime TransactionDate { get; set; } = DateTime.Today;

    [StringLength(250)]
    public string? Note { get; set; }

    [Display(Name = "Account")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select an account.")]
    public int AccountId { get; set; }
    public Account? Account { get; set; }

    [Display(Name = "Category")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public string? ReceiptPath { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped] public IFormFile? Receipt { get; set; }
}

public enum TransactionType { Income, Expense }
