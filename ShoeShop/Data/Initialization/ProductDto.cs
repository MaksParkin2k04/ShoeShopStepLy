using ShoeShop.Models;

namespace ShoeShop.Data.Initialization {
    public class ProductDto {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsSale { get; set; }
        public double Price { get; set; }
        public ProductSize Sizes { get; set; }
        public DateTime DateAdded { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public ImageDto[] Images { get; set; }
    }
}
