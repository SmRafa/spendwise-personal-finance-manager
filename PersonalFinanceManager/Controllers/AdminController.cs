using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Identity;using Microsoft.AspNetCore.Mvc;using PersonalFinanceManager.Data;
namespace PersonalFinanceManager.Controllers;
[Authorize(Roles="Admin")] public class AdminController(UserManager<IdentityUser> users,ApplicationDbContext context):Controller{public IActionResult Index()=>View(new AdminDashboardViewModel(users.Users.OrderBy(x=>x.Email).ToList(),context.Accounts.Count(),context.Transactions.Count(),context.FinancialGoals.Count()));}
public record AdminDashboardViewModel(IReadOnlyList<IdentityUser> Users,int Accounts,int Transactions,int Goals);
