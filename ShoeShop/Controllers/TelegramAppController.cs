using Microsoft.AspNetCore.Mvc;

namespace ShoeShop.Controllers {
    public class TelegramAppController : Controller {
        [Route("telegram-app")]
        public IActionResult Index() {
            return PhysicalFile(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "telegram-app", "index.html"),
                "text/html"
            );
        }
    }
}