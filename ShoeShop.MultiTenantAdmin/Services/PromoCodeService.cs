using Microsoft.EntityFrameworkCore;
using ShoeShop.MultiTenantAdmin.Data;
using ShoeShop.MultiTenantAdmin.Models;

namespace ShoeShop.MultiTenantAdmin.Services
{
    public class PromoCodeService
    {
        private readonly ApplicationContext _context;

        public PromoCodeService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<PromoCode?> GetPromoCodeAsync(string code)
        {
            return await _context.PromoCodes
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<decimal> ApplyPromoCodeAsync(string code, decimal orderAmount)
        {
            var promoCode = await GetPromoCodeAsync(code);
            
            if (promoCode == null || !promoCode.CanUse())
                return 0;

            var discount = promoCode.CalculateDiscount(orderAmount);
            return discount;
        }
        
        public async Task UsePromoCodeAsync(string code)
        {
            var promoCode = await GetPromoCodeAsync(code);
            if (promoCode != null && promoCode.CanUse())
            {
                promoCode.Use();
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<PromoCode>> GetAllPromoCodesAsync()
        {
            return await _context.PromoCodes
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PromoCode> CreatePromoCodeAsync(string code, decimal discountPercent, decimal? maxDiscountAmount = null, DateTime? expiresAt = null, int? usageLimit = null)
        {
            var promoCode = PromoCode.Create(code, discountPercent, maxDiscountAmount, expiresAt, usageLimit);
            
            _context.PromoCodes.Add(promoCode);
            await _context.SaveChangesAsync();
            
            return promoCode;
        }

        public async Task<bool> ValidatePromoCodeAsync(string code)
        {
            var promoCode = await GetPromoCodeAsync(code);
            return promoCode != null && promoCode.CanUse();
        }

        public async Task DeletePromoCodeAsync(int id)
        {
            var promoCode = await _context.PromoCodes.FindAsync(id);
            if (promoCode != null)
            {
                _context.PromoCodes.Remove(promoCode);
                await _context.SaveChangesAsync();
            }
        }
    }
}
