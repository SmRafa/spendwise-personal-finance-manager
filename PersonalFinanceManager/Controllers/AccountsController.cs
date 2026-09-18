using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;
using System.Security.Claims;

namespace PersonalFinanceManager.Controllers;

[Authorize]
public class AccountsController(ApplicationDbContext context) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index()
    {
        UserDataOwnership.EnsureUserData(context, UserId);
        var accounts = await context.Accounts.Where(a => a.UserId == UserId).OrderBy(a => a.Name).ToListAsync();
        var transactions = await context.Transactions.Where(t => t.UserId == UserId).ToListAsync();
        var transfers = await context.AccountTransfers.Where(t => t.UserId == UserId).ToListAsync();
        return View(accounts.Select(account => new AccountBalanceViewModel(account,
            account.OpeningBalance
            + transactions.Where(t => t.AccountId == account.Id && t.Type == TransactionType.Income).Sum(t => t.Amount)
            - transactions.Where(t => t.AccountId == account.Id && t.Type == TransactionType.Expense).Sum(t => t.Amount)
            + transfers.Where(t => t.ToAccountId == account.Id).Sum(t => t.Amount)
            - transfers.Where(t => t.FromAccountId == account.Id).Sum(t => t.Amount))).ToList());
    }
    public IActionResult Create() => View(new Account());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Account account)
    {
        if (!ModelState.IsValid) return View(account);
        account.UserId = UserId;
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        TempData["Success"] = "Account created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);
        return account is null ? NotFound() : View(account);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Account account)
    {
        if (id != account.Id) return NotFound();
        var existing = await context.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);
        if (existing is null) return NotFound();
        if (!ModelState.IsValid) return View(account);

        existing.Name = account.Name;
        existing.Type = account.Type;
        existing.OpeningBalance = account.OpeningBalance;
        await context.SaveChangesAsync();
        TempData["Success"] = "Account updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (await context.Transactions.AnyAsync(t => t.AccountId == id && t.UserId == UserId) ||
            await context.RecurringTransactions.AnyAsync(t => t.AccountId == id && t.UserId == UserId) ||
            await context.AccountTransfers.AnyAsync(t => (t.FromAccountId == id || t.ToAccountId == id) && t.UserId == UserId))
        {
            TempData["Success"] = "This account cannot be deleted because it has transactions.";
            return RedirectToAction(nameof(Index));
        }
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);
        if (account is not null) { context.Accounts.Remove(account); await context.SaveChangesAsync(); TempData["Success"] = "Account deleted."; }
        return RedirectToAction(nameof(Index));
    }
}
