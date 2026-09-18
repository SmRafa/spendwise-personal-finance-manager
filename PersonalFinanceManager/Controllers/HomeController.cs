using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace PersonalFinanceManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            UserDataOwnership.EnsureUserData(_context, userId);
            await RecurringTransactionService.ProcessDueTransactions(_context, userId);
            var transactions = await _context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .ToListAsync();
            var displayName = await _context.UserPreferences.Where(p => p.UserId == userId).Select(p => p.DisplayName).FirstOrDefaultAsync()
                ?? User.Identity?.Name?.Split('@')[0] ?? "there";

            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var monthTransactions = transactions.Where(t => t.TransactionDate >= startOfMonth);
            var openingBalance = await _context.Accounts.Where(a => a.UserId == userId).SumAsync(a => (decimal?)a.OpeningBalance) ?? 0;
            var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.PeriodStart == startOfMonth && b.UserId == userId)
                .OrderBy(b => b.Category!.Name)
                .ToListAsync();
            var spendingByCategory = monthTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.CategoryId)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            var chartStart = startOfMonth.AddMonths(-5);
            var monthlyChart = Enumerable.Range(0, 6).Select(index =>
            {
                var month = chartStart.AddMonths(index);
                var nextMonth = month.AddMonths(1);
                var items = transactions.Where(t => t.TransactionDate >= month && t.TransactionDate < nextMonth);
                return new MonthlyChartPoint(
                    month.ToString("MMM"),
                    items.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    items.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount));
            }).ToList();

            return View(new DashboardViewModel
            {
                DisplayName = displayName,
                NetBalance = openingBalance + totalIncome - totalExpense,
                MonthlyIncome = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                MonthlyExpense = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                RecentTransactions = transactions.Take(5).ToList(),
                BudgetLimit = budgets.Sum(b => b.Amount),
                BudgetProgress = budgets.Select(b => new BudgetProgressViewModel
                {
                    Budget = b,
                    Spent = spendingByCategory.GetValueOrDefault(b.CategoryId)
                }).ToList(),
                MonthlyChart = monthlyChart
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
