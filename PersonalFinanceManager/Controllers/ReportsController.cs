using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;
using System.Security.Claims;

namespace PersonalFinanceManager.Controllers;

[Authorize]
public class ReportsController(ApplicationDbContext context) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        UserDataOwnership.EnsureUserData(context, UserId);
        var report = await BuildReport(startDate, endDate);
        return View(report);
    }

    public async Task<IActionResult> ExportCsv(DateTime? startDate, DateTime? endDate)
    {
        UserDataOwnership.EnsureUserData(context, UserId);
        var report = await BuildReport(startDate, endDate);
        var csv = new StringBuilder();
        csv.AppendLine("Date,Type,Account,Category,Amount,Note");

        foreach (var transaction in report.Transactions.OrderBy(t => t.TransactionDate))
        {
            csv.AppendLine(string.Join(",",
                transaction.TransactionDate.ToString("yyyy-MM-dd"),
                Escape(transaction.Type.ToString()),
                Escape(transaction.Account?.Name),
                Escape(transaction.Category?.Name),
                transaction.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                Escape(transaction.Note)));
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv",
            $"finance-report-{report.StartDate:yyyyMMdd}-{report.EndDate:yyyyMMdd}.csv");
    }
    public async Task<IActionResult> Print(DateTime? startDate, DateTime? endDate)
    {
        UserDataOwnership.EnsureUserData(context, UserId);
        return View(await BuildReport(startDate, endDate));
    }

    private async Task<ReportViewModel> BuildReport(DateTime? requestedStart, DateTime? requestedEnd)
    {
        var start = requestedStart?.Date ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = requestedEnd?.Date ?? DateTime.Today;
        if (end < start) (start, end) = (end, start);

        var endExclusive = end.AddDays(1);
        var transactions = await context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.UserId == UserId && t.TransactionDate >= start && t.TransactionDate < endExclusive)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var categorySpending = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => new { Name = t.Category?.Name ?? "Uncategorized", Color = t.Category?.Color ?? "#94A3B8" })
            .Select(group => new CategorySpendingViewModel(
                group.Key.Name,
                group.Key.Color,
                group.Sum(t => t.Amount),
                totalExpense == 0 ? 0 : group.Sum(t => t.Amount) / totalExpense * 100))
            .OrderByDescending(item => item.Amount)
            .ToList();

        return new ReportViewModel
        {
            StartDate = start,
            EndDate = end,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            CategorySpending = categorySpending,
            Transactions = transactions
        };
    }

    private static string Escape(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
}
