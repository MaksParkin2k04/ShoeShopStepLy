using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoeShop.Pages {
    public class AdminModel : PageModel {
        public IActionResult OnGet() {
            // Проверяем авторизацию пользователя
            if (!User.Identity.IsAuthenticated) {
                return RedirectToPage("/Account/Login", new { ReturnUrl = "/Admin" });
            }
            
            // Проверяем админский пароль
            if (HttpContext.Session.GetString("AdminAuth") != "true") {
                return RedirectToPage("/AdminAuth", new { returnUrl = "/Admin/Index" });
            }
            
            return RedirectToPage("/Admin/Index");
        }
    }
}