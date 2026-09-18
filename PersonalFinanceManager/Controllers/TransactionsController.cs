using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;
using System.Security.Claims;

namespace PersonalFinanceManager.Controllers;

[Authorize]
public class TransactionsController(ApplicationDbContext context, IWebHostEnvironment environment) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index(string? search, int? accountId, int? categoryId, TransactionType? type, DateTime? startDate, DateTime? endDate, int page = 1)
    {
        UserDataOwnership.EnsureUserData(context, UserId);
        await RecurringTransactionService.ProcessDueTransactions(context, UserId);
        const int pageSize = 8;
        var query = context.Transactions.Include(t => t.Account).Include(t => t.Category).Where(t => t.UserId == UserId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(t => (t.Note ?? string.Empty).Contains(search) || t.Category!.Name.Contains(search));
        if (accountId.HasValue) query = query.Where(t => t.AccountId == accountId);
        if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId);
        if (type.HasValue) query = query.Where(t => t.Type == type);
        if (startDate.HasValue) query = query.Where(t => t.TransactionDate >= startDate.Value.Date);
        if (endDate.HasValue) query = query.Where(t => t.TransactionDate < endDate.Value.Date.AddDays(1));

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        await LoadDropDowns();
        return View(new TransactionListViewModel
        {
            Items = await query.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(),
            Page = page, TotalPages = totalPages, TotalCount = total,
            Search = search, AccountId = accountId, CategoryId = categoryId, Type = type, StartDate = startDate, EndDate = endDate
        });
    }

    public async Task<IActionResult> Create()
    {
        UserDataOwnership.EnsureUserData(context, UserId);
        await LoadDropDowns();
        return View(new FinanceTransaction());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FinanceTransaction transaction)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == transaction.CategoryId && c.UserId == UserId);
        if (category is not null && category.Type.ToString() != transaction.Type.ToString())
            ModelState.AddModelError(nameof(transaction.CategoryId), "Choose a category that matches the transaction type.");

        if (!ModelState.IsValid)
        {
            await LoadDropDowns();
            return View(transaction);
        }

        transaction.UserId = UserId;
        await SaveReceipt(transaction);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        TempData["Success"] = "Transaction saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var transaction = await context.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
        if (transaction is null) return NotFound();
        await LoadDropDowns();
        return View(transaction);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FinanceTransaction transaction)
    {
        if (id != transaction.Id) return NotFound();
        var existing = await context.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
        if (existing is null) return NotFound();
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == transaction.CategoryId && c.UserId == UserId);
        if (category is null || category.Type.ToString() != transaction.Type.ToString())
            ModelState.AddModelError(nameof(transaction.CategoryId), "Choose a category that matches the transaction type.");

        if (!ModelState.IsValid) { await LoadDropDowns(); return View(transaction); }

        existing.Type = transaction.Type;
        existing.Amount = transaction.Amount;
        existing.TransactionDate = transaction.TransactionDate;
        existing.Note = transaction.Note;
        existing.AccountId = transaction.AccountId;
        existing.CategoryId = transaction.CategoryId;
        await context.SaveChangesAsync();
        TempData["Success"] = "Transaction updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await context.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
        if (transaction is not null)
        {
            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
            TempData["Success"] = "Transaction deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropDowns()
    {
        ViewBag.Accounts = new SelectList(await context.Accounts.Where(a => a.UserId == UserId).OrderBy(a => a.Name).ToListAsync(), "Id", "Name");
        ViewBag.Categories = new SelectList(
            await context.Categories.Where(c => c.UserId == UserId).OrderBy(c => c.Type).ThenBy(c => c.Name).ToListAsync(),
            "Id", "Name");
    }
    private async Task SaveReceipt(FinanceTransaction transaction){if(transaction.Receipt is null||transaction.Receipt.Length==0)return;var ext=Path.GetExtension(transaction.Receipt.FileName).ToLowerInvariant();if(ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")return;var folder=Path.Combine(environment.WebRootPath,"uploads","receipts");Directory.CreateDirectory(folder);var file=$"{UserId}-{Guid.NewGuid():N}{ext}";await using var stream=System.IO.File.Create(Path.Combine(folder,file));await transaction.Receipt.CopyToAsync(stream);transaction.ReceiptPath=$"/uploads/receipts/{file}";}
}
