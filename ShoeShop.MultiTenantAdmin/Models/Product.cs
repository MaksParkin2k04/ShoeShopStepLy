namespace ShoeShop.MultiTenantAdmin.Models {
    public class Product {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public double? SalePrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        public List<ProductSize> Sizes { get; set; } = new();
        public List<ProductImage> Images { get; set; } = new();
    }
}