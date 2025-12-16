using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;
using ShoeShop.Models;
using System.Text.Json;

namespace ShoeShop.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UsersModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserInfo> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            Users = new List<UserInfo>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserInfo
                {
                    Id = user.Id.ToString(),
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "User",
                    CreatedAt = DateTime.Now,
                    EmailConfirmed = user.EmailConfirmed
                });
            }
        }

        public async Task<IActionResult> OnPostChangeRoleAsync()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            
            var userId = data.GetProperty("userId").GetString();
            var newRole = data.GetProperty("newRole").GetString();
            
            // Создаем роли если их нет
            await EnsureRolesExist();
            
            if (Guid.TryParse(userId, out var userGuid))
            {
                var user = await _userManager.FindByIdAsync(userGuid.ToString());
                if (user != null && !string.IsNullOrEmpty(newRole))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }
                    var result = await _userManager.AddToRoleAsync(user, newRole);
                    
                    // Обновляем SecurityStamp чтобы принудить перелогин
                    await _userManager.UpdateSecurityStampAsync(user);
                    
                    return new JsonResult(new { success = result.Succeeded, errors = result.Errors });
                }
            }
            
            return new JsonResult(new { success = false });
        }
        
        private async Task EnsureRolesExist()
        {
            string[] roles = { "Admin", "Manager", "Editor", "Consultant", "Analyst", "User" };
            
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new ApplicationRole { Name = role });
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteUserAsync()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            
            var userId = data.GetProperty("userId").GetString();
            
            if (Guid.TryParse(userId, out var userGuid))
            {
                var user = await _userManager.FindByIdAsync(userGuid.ToString());
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            
            return new JsonResult(new { success = true });
        }
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}