using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;
namespace PersonalFinanceManager.Controllers;
[Authorize]
public class TransfersController(ApplicationDbContext context) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index() { UserDataOwnership.EnsureUserData(context,UserId); return View(await context.AccountTransfers.Include(x=>x.FromAccount).Include(x=>x.ToAccount).Where(x=>x.UserId==UserId).OrderByDescending(x=>x.TransferDate).ToListAsync()); }
    public async Task<IActionResult> Create() { await LoadAccounts(); return View(new AccountTransfer()); }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create(AccountTransfer transfer) { if(transfer.FromAccountId==transfer.ToAccountId) ModelState.AddModelError(nameof(transfer.ToAccountId),"Choose two different accounts."); var count=await context.Accounts.CountAsync(a=>a.UserId==UserId&&(a.Id==transfer.FromAccountId||a.Id==transfer.ToAccountId)); if(count!=2) ModelState.AddModelError(string.Empty,"Select valid accounts."); if(!ModelState.IsValid){await LoadAccounts();return View(transfer);} transfer.UserId=UserId;context.AccountTransfers.Add(transfer);await context.SaveChangesAsync();TempData["Success"]="Transfer saved.";return RedirectToAction(nameof(Index));}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Delete(int id) { var item=await context.AccountTransfers.FirstOrDefaultAsync(x=>x.Id==id&&x.UserId==UserId);if(item is not null){context.AccountTransfers.Remove(item);await context.SaveChangesAsync();}return RedirectToAction(nameof(Index));}
    private async Task LoadAccounts()=>ViewBag.Accounts=new SelectList(await context.Accounts.Where(a=>a.UserId==UserId).OrderBy(a=>a.Name).ToListAsync(),"Id","Name");
}
