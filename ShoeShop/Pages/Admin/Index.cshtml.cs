using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Attributes;

namespace ShoeShop.Pages.Admin {
    [Authorize]
    [AdminAuth]
    public class AdminIndexModel : PageModel {
        public void OnGet() {
        }
        
        public IActionResult OnPostLogout() {
            HttpContext.Session.Remove("AdminAuth");
            return RedirectToPage("/Index");
        }
    }
}