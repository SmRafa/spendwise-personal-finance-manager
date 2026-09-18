namespace PersonalFinanceManager.Models;

public class DashboardViewModel
{
    public string DisplayName { get; init; } = string.Empty;
    public decimal NetBalance { get; init; }
    public decimal MonthlyIncome { get; init; }
    public decimal MonthlyExpense { get; init; }
    public decimal MonthlySavings => MonthlyIncome - MonthlyExpense;
    public IReadOnlyList<FinanceTransaction> RecentTransactions { get; init; } = [];
    public IReadOnlyList<BudgetProgressViewModel> BudgetProgress { get; init; } = [];
    public decimal BudgetLimit { get; init; }
    public IReadOnlyList<MonthlyChartPoint> MonthlyChart { get; init; } = [];
}

public record MonthlyChartPoint(string Label, decimal Income, decimal Expense);
