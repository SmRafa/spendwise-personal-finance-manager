namespace PersonalFinanceManager.Models;

public class ReportViewModel
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal NetCashFlow => TotalIncome - TotalExpense;
    public IReadOnlyList<CategorySpendingViewModel> CategorySpending { get; init; } = [];
    public IReadOnlyList<FinanceTransaction> Transactions { get; init; } = [];
}

public record CategorySpendingViewModel(string CategoryName, string Color, decimal Amount, decimal Percentage);
