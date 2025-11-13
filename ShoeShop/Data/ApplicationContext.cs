using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Models;

namespace ShoeShop.Data {
    public class ApplicationContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid> {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        public DbSet<Product> Products { get; private set; }
        public DbSet<ProductImage> ProductImages { get; private set; }
        public DbSet<Order> Orders { get; private set; }
        public DbSet<OrderDetail> OrderDetails { get; private set; }
        public DbSet<Category> Categories { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().Property(b => b.Name).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Product>().Property(b => b.Price).IsRequired();
            modelBuilder.Entity<Product>().Property(b => b.DateAdded).IsRequired();
            modelBuilder.Entity<Product>().Property(b => b.Description).IsRequired().HasMaxLength(120);
            modelBuilder.Entity<Product>().Property(b => b.Content).IsRequired();
            modelBuilder.Entity<Product>().Property(b => b.Sizes).HasField("_sizes");

            modelBuilder.Entity<ProductImage>().Property(b => b.Path).IsRequired().HasMaxLength(256);
            modelBuilder.Entity<ProductImage>().Property(b => b.Alt).IsRequired().HasMaxLength(100);

            modelBuilder.Entity<OrderDetail>().Property(o => o.Name).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<OrderDetail>().Property(o => o.Image).IsRequired().HasMaxLength(256);
            modelBuilder.Entity<OrderDetail>().Property(o => o.Price).IsRequired();
            modelBuilder.Entity<OrderDetail>().Property(o => o.Size).IsRequired();
            modelBuilder.Entity<OrderDetail>().ToTable(o => o.HasCheckConstraint("ValidPrice", $"{nameof(OrderDetail.Price)} > 0"));

            modelBuilder.Entity<Order>(o => {
                o.OwnsOne(p => p.Recipient, r => {
                    r.Property(p => p.Name).HasColumnName("RecipientName").IsRequired().HasMaxLength(50);
                    r.Property(p => p.City).HasColumnName("City").IsRequired().HasMaxLength(50);
                    r.Property(p => p.Street).HasColumnName("Street").IsRequired().HasMaxLength(256);
                    r.Property(p => p.House).HasColumnName("House").IsRequired().HasMaxLength(50);
                    r.Property(p => p.Apartment).HasColumnName("Apartment").IsRequired().HasMaxLength(50);
                    r.Property(p => p.Phone).HasColumnName("Phone").IsRequired().HasMaxLength(20);
                });

                o.Navigation(o => o.Recipient).IsRequired();
            });

            modelBuilder.Entity<Category>().Property(c => c.Name).IsRequired().HasMaxLength(100);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
