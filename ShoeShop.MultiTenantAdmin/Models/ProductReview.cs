using System.ComponentModel.DataAnnotations;
using ShoeShop.MultiTenantAdmin.Data;

namespace ShoeShop.MultiTenantAdmin.Models {
    public class ProductReview {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public Product? Product { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
