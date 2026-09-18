using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Data;

public static class RecurringTransactionService
{
    public static async Task ProcessDueTransactions(ApplicationDbContext context, string userId)
    {
        var schedules = await context.RecurringTransactions
            .Where(item => item.UserId == userId && item.IsActive && item.NextDueDate <= DateTime.Today)
            .ToListAsync();

        foreach (var schedule in schedules)
        {
            // Create one entry for each missed monthly due date, then move forward.
            while (schedule.NextDueDate <= DateTime.Today)
            {
                context.Transactions.Add(new FinanceTransaction
                {
                    UserId = userId,
                    Type = schedule.Type,
                    Amount = schedule.Amount,
                    Note = schedule.Note,
                    AccountId = schedule.AccountId,
                    CategoryId = schedule.CategoryId,
                    TransactionDate = schedule.NextDueDate
                });
                schedule.NextDueDate = schedule.NextDueDate.AddMonths(1);
            }
        }

        if (schedules.Count > 0) await context.SaveChangesAsync();
    }
}
