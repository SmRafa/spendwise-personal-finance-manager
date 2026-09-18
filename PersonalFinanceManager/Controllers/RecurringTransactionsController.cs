using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Controllers;

[Authorize]
public class RecurringTransactionsController(ApplicationDbContext context) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        UserDataOwnership.EnsureUserData(context, UserId);
        await RecurringTransactionService.ProcessDueTransactions(context, UserId);
        return View(await context.RecurringTransactions.Include(item => item.Account).Include(item => item.Category)
            .Where(item => item.UserId == UserId).OrderByDescending(item => item.IsActive).ThenBy(item => item.NextDueDate).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropDowns();
        return View(new RecurringTransaction { NextDueDate = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RecurringTransaction schedule)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == schedule.CategoryId && c.UserId == UserId);
        if (category is null || category.Type.ToString() != schedule.Type.ToString())
            ModelState.AddModelError(nameof(schedule.CategoryId), "Choose a category that matches the transaction type.");
        if (!ModelState.IsValid) { await LoadDropDowns(); return View(schedule); }

        schedule.UserId = UserId;
        context.RecurringTransactions.Add(schedule);
        await context.SaveChangesAsync();
        TempData["Success"] = "Monthly transaction scheduled.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var schedule = await context.RecurringTransactions.FirstOrDefaultAsync(item => item.Id == id && item.UserId == UserId);
        if (schedule is not null)
        {
            schedule.IsActive = !schedule.IsActive;
            await context.SaveChangesAsync();
            TempData["Success"] = schedule.IsActive ? "Schedule resumed." : "Schedule paused.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var schedule = await context.RecurringTransactions.FirstOrDefaultAsync(item => item.Id == id && item.UserId == UserId);
        if (schedule is not null) { context.RecurringTransactions.Remove(schedule); await context.SaveChangesAsync(); TempData["Success"] = "Schedule deleted."; }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropDowns()
    {
        ViewBag.Accounts = new SelectList(await context.Accounts.Where(a => a.UserId == UserId).OrderBy(a => a.Name).ToListAsync(), "Id", "Name");
        ViewBag.Categories = new SelectList(await context.Categories.Where(c => c.UserId == UserId).OrderBy(c => c.Type).ThenBy(c => c.Name).ToListAsync(), "Id", "Name");
    }
}
