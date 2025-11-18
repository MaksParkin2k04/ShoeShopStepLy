using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Services {
    public class TelegramUserService {
        private readonly ApplicationContext _context;
        
        public TelegramUserService(ApplicationContext context) {
            _context = context;
        }
        
        public async Task<TelegramUser> GetOrCreateUserAsync(long telegramId, string? firstName, string? lastName, string? username) {
            var user = await _context.TelegramUsers
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
                
            if (user == null) {
                user = new TelegramUser {
                    TelegramId = telegramId,
                    FirstName = firstName,
                    LastName = lastName,
                    Username = username,
                    CreatedDate = DateTime.Now,
                    LastActivity = DateTime.Now
                };
                
                _context.TelegramUsers.Add(user);
                await _context.SaveChangesAsync();
            } else {
                user.LastActivity = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            
            return user;
        }
        
        public async Task UpdateUserProfileAsync(long telegramId, string? phone, string? address) {
            var user = await _context.TelegramUsers
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
                
            if (user != null) {
                if (!string.IsNullOrEmpty(phone)) user.Phone = phone;
                if (!string.IsNullOrEmpty(address)) user.Address = address;
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<List<Order>> GetUserOrdersAsync(long telegramId) {
            return await _context.Orders
                .Where(o => o.TelegramUserId == telegramId)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<Order?> FindOrderByNumberAsync(string orderNumber) {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }
    }
}