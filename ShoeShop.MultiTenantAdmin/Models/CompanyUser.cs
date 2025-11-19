using System.ComponentModel.DataAnnotations;

namespace ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Models {
    public class CompanyUser {
        public Guid Id { get; set; }
        
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Admin";
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
    }
}
