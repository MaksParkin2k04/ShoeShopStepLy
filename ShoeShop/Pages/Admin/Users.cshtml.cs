using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;

namespace ShoeShop.Pages.Admin;

public class UsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public List<UserViewModel> Users { get; set; } = new();

    public async Task OnGetAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        
        foreach (var user in users)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            Users.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd,
                IsAdmin = isAdmin,
                CreatedAt = DateTime.Now
            });
        }
    }

    public async Task<IActionResult> OnPostBlockAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            TempData["Error"] = "Пользователь не найден";
            return RedirectToPage();
        }

        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(100));
        TempData["Success"] = "Пользователь заблокирован";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnblockAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            TempData["Error"] = "Пользователь не найден";
            return RedirectToPage();
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        TempData["Success"] = "Пользователь разблокирован";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            TempData["Error"] = "Пользователь не найден";
            return RedirectToPage();
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "Пользователь удален";
        }
        else
        {
            TempData["Error"] = "Ошибка удаления пользователя";
        }
        
        return RedirectToPage();
    }

    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
