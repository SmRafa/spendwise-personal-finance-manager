using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Models;
namespace PersonalFinanceManager.Controllers;

[Authorize]
public class SettingsController(ApplicationDbContext context, IWebHostEnvironment environment, UserManager<IdentityUser> users, SignInManager<IdentityUser> signInManager) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index() => View(await context.UserPreferences.FirstOrDefaultAsync(x=>x.UserId==UserId) ?? new UserPreference {UserId=UserId,DisplayName=User.Identity?.Name?.Split('@')[0]??""});
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UserPreference model)
    {
        var item=await context.UserPreferences.FirstOrDefaultAsync(x=>x.UserId==UserId);
        if(!ModelState.IsValid){model.ProfileImagePath=item?.ProfileImagePath;return View(model);}
        if(item is null){item=model;item.UserId=UserId;context.UserPreferences.Add(item);}
        else{item.DisplayName=model.DisplayName;item.Currency=model.Currency;item.Theme=model.Theme;item.BudgetAlerts=model.BudgetAlerts;item.GoalAlerts=model.GoalAlerts;}
        if(model.ProfileImage is not null&&model.ProfileImage.Length>0)
        {
            var ext=Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();
            if(model.ProfileImage.Length>2*1024*1024||ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp"){ModelState.AddModelError(nameof(model.ProfileImage),"Use JPG, PNG, or WEBP under 2 MB.");model.ProfileImagePath=item.ProfileImagePath;return View(model);}
            var folder=Path.Combine(environment.WebRootPath,"uploads","avatars");Directory.CreateDirectory(folder);
            var fileName=$"{UserId}{ext}";await using var stream=System.IO.File.Create(Path.Combine(folder,fileName));await model.ProfileImage.CopyToAsync(stream);item.ProfileImagePath=$"/uploads/avatars/{fileName}";
        }
        await context.SaveChangesAsync();TempData["Success"]="Settings saved successfully.";return RedirectToAction(nameof(Index));
    }
    public IActionResult ChangePassword()=>View(new ChangePasswordViewModel());
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if(!ModelState.IsValid)return View(model);var user=await users.GetUserAsync(User);if(user is null)return Challenge();
        var result=await users.ChangePasswordAsync(user,model.CurrentPassword,model.NewPassword);
        if(!result.Succeeded){foreach(var error in result.Errors)ModelState.AddModelError(string.Empty,error.Description);return View(model);}
        await signInManager.RefreshSignInAsync(user);TempData["Success"]="Password changed successfully.";return RedirectToAction(nameof(Index));
    }
}
