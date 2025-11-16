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

        public async Task OnGetAsync() {
            BasketShopping bs = basketShopping.GetBasketShopping();
            Guid[] productIds = bs.Products.Select(p => p.ProductId).ToArray();
            Products = await repository.GetProducts(productIds);
        }

        public async Task<IActionResult> OnPostAsync(string name, string city, string street, string house, string apartment, string phone, string coment, Guid[] products) {

            BasketShopping bs = basketShopping.GetBasketShopping();
            if (bs.Products == null || bs.Products.Count == 0) {
                return RedirectToPage("/BasketShopping");
            }

            Guid[] productIds = bs.Products.Select(p => p.ProductId).ToArray();
            IEnumerable<Product> prod = await repository.GetProducts(productIds);

            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach (BasketItem basketItem in bs.Products) {
                Product? product = prod.FirstOrDefault(p => p.Id == basketItem.ProductId);
                if (product != null) {
                    string imagePath = product.Images?.Count > 0 ? product.Images[0].Path : "images/no-image.jpg";
                    orderDetails.Add(OrderDetail.Create(product.Id, imagePath, product.Name ?? "", product.Price, basketItem.Size));
                }
            }

            if (orderDetails.Count == 0) {
                return RedirectToPage("/BasketShopping");
            }

            ApplicationUser? user = await userManager.GetUserAsync(User);

            OrderRecipient recipient = OrderRecipient.Create(name ?? "", city ?? "", street ?? "", house ?? "", apartment ?? "", phone ?? "");
            Order order = Order.Create(user!.Id, DateTime.Now, coment ?? "", recipient, orderDetails);

            await repository.CreateOrder(order);
            
            // Уменьшаем остатки товаров
            foreach (BasketItem basketItem in bs.Products) {
                await stockService.ReduceStockSafeAsync(basketItem.ProductId, basketItem.Size);
            }
            
            basketShopping.Clear();

            return RedirectToPage("/Order", new { orderId = order.Id });
        }
    }
}
