using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoeShop.Pages.Admin {
    [Authorize(Roles = "Admin")]
    public class AdminIndexModel : PageModel {
        public void OnGet() {
        }
    }
}