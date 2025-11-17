using Microsoft.AspNetCore.Identity;
using ShoeShop.Models;

namespace ShoeShop.Data {
    public class ApplicationUser : IdentityUser<Guid> {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Street { get; set; }
        public string? House { get; set; }
        public string? Apartment { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
    }
}
