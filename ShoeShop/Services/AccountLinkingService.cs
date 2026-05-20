using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Services {
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
            
            
           
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<List<Order>> GetUnifiedOrdersAsync(long telegramId) {
           
                 
            
            var orders = new List<Order>();
            
          
            return orders.OrderByDescending(o => o.CreatedDate).ToList();
        }
        

    }
}