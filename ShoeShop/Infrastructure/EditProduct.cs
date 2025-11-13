using ShoeShop.Models;

namespace ShoeShop.Infrastructure {
    public class EditProduct {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public bool? IsSale { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public double? Price { get; set; }
        public ProductSize? Sizes { get; set; }
        public EditImage[]? Images { get; set; }
        public Guid? CategoryId { get; set; }

        public void Validate() {
            if (Name == null) { throw new ArgumentOutOfRangeException(nameof(Name)); }
            if (IsSale == null) { throw new ArgumentOutOfRangeException(nameof(IsSale)); }
            if (Price == null) { throw new ArgumentOutOfRangeException(nameof(Price)); }
            if (Sizes == null) { throw new ArgumentOutOfRangeException(nameof(Sizes)); }
            if (Description == null) { throw new ArgumentOutOfRangeException(nameof(Description)); }
            if (Content == null) { throw new ArgumentOutOfRangeException(nameof(Content)); }
        }
    }
}
