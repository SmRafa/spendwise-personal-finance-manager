using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Models;

public class Account
{
    public int Id { get; set; }
    [Required, StringLength(60), Display(Name = "Account name")]
    public string Name { get; set; } = string.Empty;
    [Display(Name = "Account type")]
    public AccountType Type { get; set; }
    [Range(0, 9_999_999_999), Display(Name = "Opening balance")]
    public decimal OpeningBalance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
}

public enum AccountType { Cash, Bank, Easypaisa, JazzCash }
