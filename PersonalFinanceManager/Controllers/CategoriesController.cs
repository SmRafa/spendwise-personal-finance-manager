using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;
using System.Security.Claims;

namespace PersonalFinanceManager.Controllers;

[Authorize]
public class CategoriesController(ApplicationDbContext context) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index()
    {
        UserDataOwnership.EnsureUserData(context, UserId);
        return View(await context.Categories.Where(c => c.UserId == UserId).OrderBy(c => c.Type).ThenBy(c => c.Name).ToListAsync());
    }

    public IActionResult Create() => View(new Category());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid) return View(category);
        category.UserId = UserId;
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        TempData["Success"] = "Category created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return NotFound();
        var existing = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (existing is null) return NotFound();
        if (!ModelState.IsValid) return View(category);

        existing.Name = category.Name;
        existing.Type = category.Type;
        existing.Color = category.Color;
        await context.SaveChangesAsync();
        TempData["Success"] = "Category updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (await context.Transactions.AnyAsync(t => t.CategoryId == id && t.UserId == UserId) ||
            await context.RecurringTransactions.AnyAsync(t => t.CategoryId == id && t.UserId == UserId))
        {
            TempData["Success"] = "This category cannot be deleted because it has transactions.";
            return RedirectToAction(nameof(Index));
        }
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (category is not null)
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            TempData["Success"] = "Category deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
