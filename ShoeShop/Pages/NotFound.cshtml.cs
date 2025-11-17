using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoeShop.Pages {
    public class NotFoundModel : PageModel {
        public void OnGet() {
            Response.StatusCode = 404;
        }
    }
}