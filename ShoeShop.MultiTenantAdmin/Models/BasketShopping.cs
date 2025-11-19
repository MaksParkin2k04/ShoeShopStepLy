
namespace ShoeShop.MultiTenantAdmin.Models {
    public class BasketShopping {
        public List<BasketItem> Products { get; set; } = new List<BasketItem>();
    }

    public class BasketItem {
        public Guid ProductId { get; set; }
        public int Size { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
