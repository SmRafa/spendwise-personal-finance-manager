using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Category type")]
    public CategoryType Type { get; set; }

    [Required, RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a hex color, for example #605CFF.")]
    public string Color { get; set; } = "#605CFF";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
}

public enum CategoryType { Income, Expense }
