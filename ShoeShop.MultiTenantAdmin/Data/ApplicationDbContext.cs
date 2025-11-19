using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Models;

namespace ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Data {
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid> {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyUser> CompanyUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<Company>()
                .HasIndex(c => c.ShortName)
                .IsUnique();

            builder.Entity<CompanyUser>()
                .HasOne(cu => cu.Company)
                .WithMany()
                .HasForeignKey(cu => cu.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CompanyUser>()
                .HasOne(cu => cu.User)
                .WithMany()
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CompanyUser>()
                .HasIndex(cu => new { cu.CompanyId, cu.UserId })
                .IsUnique();
        }
    }
}
