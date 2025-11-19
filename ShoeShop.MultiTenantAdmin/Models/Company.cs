using System.ComponentModel.DataAnnotations;

namespace ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Models {
    public class Company {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string ShortName { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? ContactPhone { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
    }
}
