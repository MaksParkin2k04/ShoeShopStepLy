using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin {
    public class LoginModel : PageModel {
        public string? ErrorMessage { get; set; }

        public void OnGet() {
            // Проверяем, уже авторизован ли как админ
            if (HttpContext.Session.GetString("AdminAuth") == "true") {
                Response.Redirect("/Admin/Index");
            }
        }

        public IActionResult OnPost(string password) {
            if (password == "qwerty") {
                HttpContext.Session.SetString("AdminAuth", "true");
                return RedirectToPage("/Admin/Index");
            } else {
                ErrorMessage = "Неверный пароль";
                return Page();
            }
        }
    }
}
