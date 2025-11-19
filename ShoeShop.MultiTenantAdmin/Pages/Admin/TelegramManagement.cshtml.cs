using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoeShop.MultiTenantAdmin.Data;
using ShoeShop.MultiTenantAdmin.Models;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin {
    public class TelegramManagementModel : PageModel {
        private readonly ApplicationContext _context;
        
        public TelegramManagementModel(ApplicationContext context) {
            _context = context;
        }
        
        public int TotalUsers { get; set; }
        public int TelegramOrders { get; set; }
        public decimal TelegramRevenue { get; set; }
        public int TodayActivity { get; set; }
        
        public List<TelegramUser> TelegramUsers { get; set; } = new();
        public List<Order> RecentTelegramOrders { get; set; } = new();
        
        public async Task OnGetAsync() {
            // Статистика пользователей
            TotalUsers = await _context.TelegramUsers.CountAsync();
            
            // Статистика заказов из Telegram
            var telegramOrders = await _context.Orders
                .Where(o => o.Source == "Telegram")
                .Include(o => o.OrderDetails)
                .ToListAsync();
                
            TelegramOrders = telegramOrders.Count;
            TelegramRevenue = (decimal)telegramOrders
                .SelectMany(o => o.OrderDetails ?? new List<OrderDetail>())
                .Sum(d => d.Price);
            
            // Активность сегодня
            TodayActivity = await _context.TelegramUsers
                .CountAsync(u => u.LastActivity.Date == DateTime.Today);
            
            // Пользователи Telegram с заказами
            TelegramUsers = await _context.TelegramUsers
                .OrderByDescending(u => u.LastActivity)
                .Take(20)
                .ToListAsync();
                
            // Загружаем заказы для каждого пользователя
            foreach (var user in TelegramUsers) {
                user.Orders = await _context.Orders
                    .Where(o => o.TelegramUserId == user.TelegramId)
                    .ToListAsync();
            }
            
            // Последние заказы из Telegram
            RecentTelegramOrders = await _context.Orders
                .Where(o => o.Source == "Telegram")
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.CreatedDate)
                .Take(10)
                .ToListAsync();
        }
    }
}
