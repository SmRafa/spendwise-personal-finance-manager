using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;
using System.Security.Claims;

namespace PersonalFinanceManager.Controllers;

[Authorize]
public class BudgetsController(ApplicationDbContext context) : Controller
{
    private static DateTime CurrentMonth => new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        UserDataOwnership.EnsureUserData(context, UserId);
        var budgets = await context.Budgets
            .Include(b => b.Category)
            .Where(b => b.PeriodStart == CurrentMonth && b.UserId == UserId)
            .OrderBy(b => b.Category!.Name)
            .ToListAsync();

        var monthEnd = CurrentMonth.AddMonths(1);
        var spending = await context.Transactions
            .Where(t => t.UserId == UserId && t.Type == TransactionType.Expense && t.TransactionDate >= CurrentMonth && t.TransactionDate < monthEnd)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Amount = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Amount);

        return View(budgets.Select(b => new BudgetProgressViewModel
        {
            Budget = b,
            Spent = spending.GetValueOrDefault(b.CategoryId)
        }).ToList());
    }

    public async Task<IActionResult> Create()
    {
        UserDataOwnership.EnsureUserData(context, UserId);
        await LoadExpenseCategories();
        return View(new Budget { PeriodStart = CurrentMonth });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Budget budget)
    {
        budget.PeriodStart = CurrentMonth;
        if (await context.Budgets.AnyAsync(b => b.CategoryId == budget.CategoryId && b.PeriodStart == CurrentMonth && b.UserId == UserId))
            ModelState.AddModelError(nameof(budget.CategoryId), "A budget for this category already exists this month.");

        if (!ModelState.IsValid)
        {
            await LoadExpenseCategories();
            return View(budget);
        }

        budget.UserId = UserId;
        context.Budgets.Add(budget);
        await context.SaveChangesAsync();
        TempData["Success"] = "Monthly budget saved.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var budget = await context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (budget is null) return NotFound();
        await LoadExpenseCategories();
        return View(budget);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Budget budget)
    {
        if (id != budget.Id) return NotFound();
        var existing = await context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (existing is null) return NotFound();
        if (!ModelState.IsValid) { await LoadExpenseCategories(); return View(budget); }

        existing.Amount = budget.Amount;
        await context.SaveChangesAsync();
        TempData["Success"] = "Budget updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var budget = await context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (budget is not null)
        {
            context.Budgets.Remove(budget);
            await context.SaveChangesAsync();
            TempData["Success"] = "Budget removed.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadExpenseCategories() =>
        ViewBag.Categories = new SelectList(
            await context.Categories.Where(c => c.Type == CategoryType.Expense && c.UserId == UserId).OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name");
}
