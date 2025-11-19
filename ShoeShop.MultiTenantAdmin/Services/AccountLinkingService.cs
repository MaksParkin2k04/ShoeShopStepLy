using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeShop.MultiTenantAdmin.Data;
using ShoeShop.MultiTenantAdmin.Models;

namespace ShoeShop.MultiTenantAdmin.Services {
    public class AccountLinkingService {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public AccountLinkingService(ApplicationContext context, UserManager<ApplicationUser> userManager) {
            _context = context;
            _userManager = userManager;
        }
        
        public async Task<bool> LinkAccountsAsync(long telegramId, string email, string password) {
            var webUser = await _userManager.FindByEmailAsync(email);
            if (webUser == null) return false;
            
            var passwordValid = await _userManager.CheckPasswordAsync(webUser, password);
            if (!passwordValid) return false;
            
            var telegramUser = await _context.TelegramUsers
                .FirstOrDefaultAsync(t => t.TelegramId == telegramId);
                
            if (telegramUser == null) return false;
            
            telegramUser.WebUserId = webUser.Id;
            telegramUser.Email = email;
            telegramUser.IsLinkedToWebsite = true;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<List<Order>> GetUnifiedOrdersAsync(long telegramId) {
            var telegramUser = await _context.TelegramUsers
                .FirstOrDefaultAsync(t => t.TelegramId == telegramId);
                
            if (telegramUser == null) return new List<Order>();
            
            var orders = new List<Order>();
            
            // Заказы из Telegram
            var telegramOrders = await _context.Orders
                .Where(o => o.TelegramUserId == telegramId)
                .Include(o => o.OrderDetails)
                .ToListAsync();
            orders.AddRange(telegramOrders);
            
            // Если аккаунт связан, добавляем заказы с сайта
            if (telegramUser.IsLinkedToWebsite && telegramUser.WebUserId.HasValue) {
                var webOrders = await _context.Orders
                    .Where(o => o.CustomerId == telegramUser.WebUserId.Value && o.TelegramUserId == null)
                    .Include(o => o.OrderDetails)
                    .ToListAsync();
                orders.AddRange(webOrders);
            }
            
            return orders.OrderByDescending(o => o.CreatedDate).ToList();
        }
        
        public async Task<bool> IsAccountLinkedAsync(long telegramId) {
            var telegramUser = await _context.TelegramUsers
                .FirstOrDefaultAsync(t => t.TelegramId == telegramId);
                
            return telegramUser?.IsLinkedToWebsite == true;
        }
    }
}
