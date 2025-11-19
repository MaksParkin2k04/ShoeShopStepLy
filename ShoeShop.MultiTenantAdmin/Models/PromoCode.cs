using System.ComponentModel.DataAnnotations;

namespace ShoeShop.MultiTenantAdmin.Models
{
    public class PromoCode
    {
        public int Id { get; set; }
        
        [Required]
        public string Code { get; set; } = string.Empty;
        
        public decimal DiscountPercent { get; set; }
        
        public decimal? MaxDiscountAmount { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
        
        public int? UsageLimit { get; set; }
        
        public int UsageCount { get; set; }

        public static PromoCode Create(string code, decimal discountPercent, decimal? maxDiscountAmount = null, DateTime? expiresAt = null, int? usageLimit = null)
        {
            return new PromoCode
            {
                Code = code,
                DiscountPercent = discountPercent,
                MaxDiscountAmount = maxDiscountAmount,
                IsActive = true,
                CreatedAt = DateTime.Now,
                ExpiresAt = expiresAt,
                UsageLimit = usageLimit,
                UsageCount = 0
            };
        }

        public bool CanUse()
        {
            return IsActive && 
                   (ExpiresAt == null || ExpiresAt > DateTime.Now) &&
                   (UsageLimit == null || UsageCount < UsageLimit);
        }

        public decimal CalculateDiscount(decimal orderAmount)
        {
            var discount = orderAmount * (DiscountPercent / 100);
            if (MaxDiscountAmount.HasValue && discount > MaxDiscountAmount.Value)
                discount = MaxDiscountAmount.Value;
            return discount;
        }

        public void Use()
        {
            UsageCount++;
        }
    }
}
