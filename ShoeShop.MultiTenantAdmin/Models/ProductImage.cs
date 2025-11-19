namespace ShoeShop.MultiTenantAdmin.Models {
    public class ProductImage {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Alt { get; set; } = string.Empty;
    }
}