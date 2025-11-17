using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoeShop.Pages {
    public class AdminAuthModel : PageModel {
        public string? ErrorMessage { get; set; }
        public string ReturnUrl { get; set; } = "/Admin/Index";

        public void OnGet(string? returnUrl = null) {
            ReturnUrl = returnUrl ?? "/Admin/Index";
            
            // Если уже авторизован, перенаправляем
            if (HttpContext.Session.GetString("AdminAuth") == "true") {
                Response.Redirect(ReturnUrl);
            }
        }

        public IActionResult OnPost(string password, string? returnUrl = null) {
            ReturnUrl = returnUrl ?? "/Admin/Index";
            
            if (password == "qwerty") {
                HttpContext.Session.SetString("AdminAuth", "true");
                return Redirect(ReturnUrl);
            } else {
                ErrorMessage = "Неверный пароль";
                return Page();
            }
        }
    }
}