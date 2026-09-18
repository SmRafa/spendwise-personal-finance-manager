namespace PersonalFinanceManager.Models;

public class BudgetProgressViewModel
{
    public Budget Budget { get; init; } = new();
    public decimal Spent { get; init; }
    public decimal Remaining => Budget.Amount - Spent;
    public int Percentage => Budget.Amount == 0 ? 0 : Math.Min(100, (int)Math.Round(Spent / Budget.Amount * 100));
}
