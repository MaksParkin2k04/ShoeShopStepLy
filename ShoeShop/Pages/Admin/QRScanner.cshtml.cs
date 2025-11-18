using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoeShop.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class QRScannerModel : PageModel
    {
        public void OnGet()
        {
            // Страница для сканирования QR-кодов
        }
    }
}