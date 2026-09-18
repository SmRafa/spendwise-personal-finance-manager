using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Data;

public static class UserDataOwnership
{
    public static void EnsureUserData(ApplicationDbContext context, string userId)
    {
        // Existing single-user data is claimed by the first account that signs in.
        // All records created afterwards receive an explicit UserId.
        context.Database.ExecuteSqlInterpolated($"UPDATE [Accounts] SET [UserId] = {userId} WHERE [UserId] IS NULL");
        context.Database.ExecuteSqlInterpolated($"UPDATE [Categories] SET [UserId] = {userId} WHERE [UserId] IS NULL");
        context.Database.ExecuteSqlInterpolated($"UPDATE [Transactions] SET [UserId] = {userId} WHERE [UserId] IS NULL");
        context.Database.ExecuteSqlInterpolated($"UPDATE [Budgets] SET [UserId] = {userId} WHERE [UserId] IS NULL");
        context.Database.ExecuteSqlInterpolated($"UPDATE [RecurringTransactions] SET [UserId] = {userId} WHERE [UserId] IS NULL");
        context.Database.ExecuteSqlInterpolated($"UPDATE [AccountTransfers] SET [UserId] = {userId} WHERE [UserId] IS NULL");
        context.Database.ExecuteSqlInterpolated($"UPDATE [FinancialGoals] SET [UserId] = {userId} WHERE [UserId] IS NULL");

        if (context.Categories.Any(category => category.UserId == userId)) return;

        context.Categories.AddRange(
            new Category { UserId = userId, Name = "Salary", Type = CategoryType.Income, Color = "#16A34A" },
            new Category { UserId = userId, Name = "Freelance", Type = CategoryType.Income, Color = "#0891B2" },
            new Category { UserId = userId, Name = "Other income", Type = CategoryType.Income, Color = "#7C3AED" },
            new Category { UserId = userId, Name = "Food & dining", Type = CategoryType.Expense, Color = "#F97316" },
            new Category { UserId = userId, Name = "Transport", Type = CategoryType.Expense, Color = "#0284C7" },
            new Category { UserId = userId, Name = "Bills & utilities", Type = CategoryType.Expense, Color = "#8B5CF6" },
            new Category { UserId = userId, Name = "Shopping", Type = CategoryType.Expense, Color = "#EC4899" },
            new Category { UserId = userId, Name = "Health", Type = CategoryType.Expense, Color = "#EF4444" },
            new Category { UserId = userId, Name = "Entertainment", Type = CategoryType.Expense, Color = "#EAB308" }
        );
        context.SaveChanges();
    }
}
