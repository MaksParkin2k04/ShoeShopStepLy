using System.ComponentModel.DataAnnotations;
using ShoeShop.MultiTenantAdmin.Data;

namespace ShoeShop.MultiTenantAdmin.Models {
    public class ReviewReply {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public string AdminId { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Reply { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public ProductReview? Review { get; set; }
        public ApplicationUser? Admin { get; set; }
    }
}
