using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.Data;
using Microsoft.EntityFrameworkCore;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin {
    public class TelegramBotModel : PageModel {
        private readonly ApplicationContext _context;
        
        public TelegramBotModel(ApplicationContext context) {
            _context = context;
        }
        
        public int TotalOrders { get; set; }
        public double TotalAmount { get; set; }
        public int UniqueCustomers { get; set; }
        public int TodayOrders { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        
        public async Task OnGetAsync() {
            var telegramOrders = await _context.Orders
                .Where(o => o.Coment != null && o.Coment.Contains("Заказ из Telegram"))
                .Include(o => o.OrderDetails)
                .Include(o => o.Recipient)
                .ToListAsync();
            
            TotalOrders = telegramOrders.Count;
            TotalAmount = telegramOrders.Sum(o => o.OrderDetails?.Sum(od => od.Price) ?? 0);
            
            // Подсчет уникальных клиентов по Telegram ID
            var uniqueTelegramIds = telegramOrders
                .Where(o => o.Coment != null)
                .Select(o => ExtractTelegramId(o.Coment))
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .Count();
            UniqueCustomers = uniqueTelegramIds;
            
            var today = DateTime.Today;
            TodayOrders = telegramOrders.Count(o => o.CreatedDate.Date == today);
            
            RecentOrders = telegramOrders
                .OrderByDescending(o => o.CreatedDate)
                .Take(10)
                .ToList();
        }
        
        private string ExtractTelegramId(string comment) {
            if (string.IsNullOrEmpty(comment)) return "";
            
            var startIndex = comment.IndexOf("ID пользователя: ");
            if (startIndex == -1) return "";
            
            startIndex += "ID пользователя: ".Length;
            var endIndex = comment.IndexOf(" ", startIndex);
            if (endIndex == -1) endIndex = comment.Length;
            
            return comment.Substring(startIndex, endIndex - startIndex);
        }
    }
}
