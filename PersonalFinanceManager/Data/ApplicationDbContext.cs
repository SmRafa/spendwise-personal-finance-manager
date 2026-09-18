using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<FinanceTransaction> Transactions => Set<FinanceTransaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<RecurringTransaction> RecurringTransactions => Set<RecurringTransaction>();
    public DbSet<AccountTransfer> AccountTransfers => Set<AccountTransfer>();
    public DbSet<FinancialGoal> FinancialGoals => Set<FinancialGoal>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
}
