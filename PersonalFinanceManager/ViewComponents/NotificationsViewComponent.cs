using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;
namespace PersonalFinanceManager.ViewComponents;
public class NotificationsViewComponent(ApplicationDbContext context):ViewComponent
{
 public async Task<IViewComponentResult> InvokeAsync()
 {
  var userId=HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);if(string.IsNullOrEmpty(userId))return View(Array.Empty<NotificationItem>());
  var pref=await context.UserPreferences.FirstOrDefaultAsync(p=>p.UserId==userId);var month=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);var end=month.AddMonths(1);var items=new List<NotificationItem>();
  if(pref?.BudgetAlerts!=false){var budgets=await context.Budgets.Include(b=>b.Category).Where(b=>b.UserId==userId&&b.PeriodStart==month).ToListAsync();var spend=await context.Transactions.Where(t=>t.UserId==userId&&t.Type==TransactionType.Expense&&t.TransactionDate>=month&&t.TransactionDate<end).GroupBy(t=>t.CategoryId).Select(g=>new{Id=g.Key,Amount=g.Sum(t=>t.Amount)}).ToDictionaryAsync(x=>x.Id,x=>x.Amount);items.AddRange(budgets.Where(b=>spend.GetValueOrDefault(b.CategoryId)/b.Amount>=.8m).Select(b=>new NotificationItem($"{b.Category!.Name} budget is {Math.Round(spend.GetValueOrDefault(b.CategoryId)/b.Amount*100)}% used",$"Rs. {spend.GetValueOrDefault(b.CategoryId):N0} of Rs. {b.Amount:N0} spent","Budgets","Index")));}
  if(pref?.GoalAlerts!=false){var goals=await context.FinancialGoals.Where(g=>g.UserId==userId&&g.CurrentAmount<g.TargetAmount&&g.TargetDate!=null&&g.TargetDate<=DateTime.Today.AddDays(14)).ToListAsync();items.AddRange(goals.Select(g=>new NotificationItem($"{g.Name} deadline is near",$"Rs. {g.TargetAmount-g.CurrentAmount:N0} still needed","Goals","Index")));}
  return View(items.Take(5).ToList());
 }
}
