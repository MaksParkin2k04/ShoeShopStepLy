
namespace ShoeShop.Models {
    public class BasketShopping {
        public List<BasketItem> Products { get; set; } = new List<BasketItem>();
    }

    public class BasketItem {
        public Guid ProductId { get; set; }
        public int Size { get; set; }
    }
}
