using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Data;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Pages {
    [Authorize]
    public class CreatingOrderModel : PageModel {
        public CreatingOrderModel(UserManager<ApplicationUser> userManager, IProductRepository repository, IBasketShoppingService basketShopping, StockService stockService) {
            this.userManager = userManager;
            this.repository = repository;
            this.basketShopping = basketShopping;
            this.stockService = stockService;
        }

        private UserManager<ApplicationUser> userManager;
        private IProductRepository repository;
        private readonly IBasketShoppingService basketShopping;
        private readonly StockService stockService;


        public IEnumerable<Product>? Products { get; private set; }
        public List<BasketProductInfo> BasketItems { get; private set; } = new List<BasketProductInfo>();

        public async Task OnGetAsync() {
            BasketShopping bs = basketShopping.GetBasketShopping();
            Guid[] productIds = bs.Products.Select(p => p.ProductId).ToArray();
            Products = await repository.GetProducts(productIds);
            
            List<BasketProductInfo> list = new List<BasketProductInfo>();
            foreach (BasketItem item in bs.Products) {
                Product? product = Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null) {
                    list.Add(new BasketProductInfo { Product = product, Size = item.Size, Quantity = item.Quantity });
                }
            }
            BasketItems = list;
        }

        public async Task<IActionResult> OnPostAsync(string name, string city, string street, string house, string apartment, string phone, string coment, Guid[] products) {

            BasketShopping bs = basketShopping.GetBasketShopping();
            if (bs.Products == null || bs.Products.Count == 0) {
                return RedirectToPage("/BasketShopping");
            }

            // Проверяем наличие товаров
            foreach (BasketItem basketItem in bs.Products) {
                bool isAvailable = await stockService.IsAvailableAsync(basketItem.ProductId, basketItem.Size, basketItem.Quantity);
                if (!isAvailable) {
                    TempData["Error"] = $"Недостаточно товара на складе. Проверьте корзину.";
                    return RedirectToPage("/BasketShopping");
                }
            }

            Guid[] productIds = bs.Products.Select(p => p.ProductId).ToArray();
            IEnumerable<Product> prod = await repository.GetProducts(productIds);

            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach (BasketItem basketItem in bs.Products) {
                Product? product = prod.FirstOrDefault(p => p.Id == basketItem.ProductId);
                if (product != null) {
                    string imagePath = product.Images?.Count > 0 ? product.Images[0].Path : "images/no-image.jpg";
                    for (int i = 0; i < basketItem.Quantity; i++) {
                        orderDetails.Add(OrderDetail.Create(product.Id, imagePath, product.Name ?? "", product.Price, basketItem.Size));
                    }
                }
            }

            if (orderDetails.Count == 0) {
                return RedirectToPage("/BasketShopping");
            }

            ApplicationUser? user = await userManager.GetUserAsync(User);

            OrderRecipient recipient = OrderRecipient.Create(name ?? "", city ?? "", street ?? "", house ?? "", apartment ?? "", phone ?? "");
            Order order = Order.Create(user!.Id, DateTime.Now, coment ?? "", recipient, orderDetails, PaymentType.Cash);

            await repository.CreateOrder(order);
            
            // Уменьшаем остатки товаров
            foreach (BasketItem basketItem in bs.Products) {
                for (int i = 0; i < basketItem.Quantity; i++) {
                    await stockService.ReduceStockSafeAsync(basketItem.ProductId, basketItem.Size);
                }
            }
            
            basketShopping.Clear();

            return RedirectToPage("/Order", new { orderId = order.Id });
        }
    }
}
