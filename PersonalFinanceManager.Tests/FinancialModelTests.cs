using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Tests;

public class FinancialModelTests
{
    [Fact]
    public void Goal_percentage_is_calculated_correctly()
    {
        var goal = new FinancialGoal { TargetAmount = 100_000m, CurrentAmount = 25_000m };
        Assert.Equal(25, goal.Percentage);
    }

    [Fact]
    public void Goal_percentage_is_capped_at_100()
    {
        var goal = new FinancialGoal { TargetAmount = 10_000m, CurrentAmount = 15_000m };
        Assert.Equal(100, goal.Percentage);
    }

    [Fact]
    public void Monthly_savings_equals_income_minus_expenses()
    {
        var dashboard = new DashboardViewModel { MonthlyIncome = 185_000m, MonthlyExpense = 76_420m };
        Assert.Equal(108_580m, dashboard.MonthlySavings);
    }

    [Fact]
    public void Account_balance_viewmodel_keeps_the_calculated_balance()
    {
        var account = new Account { Name = "Cash", OpeningBalance = 5_000m };
        var balance = new AccountBalanceViewModel(account, 7_500m);
        Assert.Equal(7_500m, balance.CurrentBalance);
        Assert.Equal("Cash", balance.Account.Name);
    }
}
