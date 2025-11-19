using Microsoft.AspNetCore.Identity;

namespace ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Models {
    public class ApplicationUser : IdentityUser<Guid> {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
