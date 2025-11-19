using ShoeShop.MultiTenantAdmin.Models;
using Microsoft.EntityFrameworkCore;

namespace ShoeShop.MultiTenantAdmin.Data {
    public class CustomerRepository : ICustomerRepository {
        private readonly ApplicationContext _context;
        
        public CustomerRepository(ApplicationContext context) {
            _context = context;
        }
        
        public async Task AddOrderAsync(Order order) {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }
        
        public async Task<ICollection<Order>> GetOrdersByTelegramIdAsync(string telegramId) {
            return await _context.Orders
                .Where(o => o.Coment != null && o.Coment.Contains($"ID пользователя: {telegramId}"))
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }
    }
}
