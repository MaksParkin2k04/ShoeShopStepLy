using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Pages {
    public class ProductModel : PageModel {
        public ProductModel(IProductRepository repository, IBasketShoppingService basketShopping, StockService stockService) {
            this.repository = repository;
            this.basketShopping = basketShopping;
            this.stockService = stockService;
        }

        private IProductRepository repository;
        private readonly IBasketShoppingService basketShopping;
        private readonly StockService stockService;

        public Product? Product { get; private set; }
        public ProductAvailabilityStatus AvailabilityStatus { get; private set; }
        public Dictionary<int, int> SizeQuantities { get; private set; } = new();

        public async Task OnGetAsync(Guid id) {
            Product = await repository.GetProduct(id);
            if (Product != null) {
                AvailabilityStatus = await stockService.GetAvailabilityStatusAsync(Product.Id);
                SizeQuantities = await stockService.GetSizeQuantitiesAsync(Product.Id);
            }
        }

        public async Task<IActionResult> OnPostAsync(Guid productId, int selectedSize) {
            // Проверяем наличие товара
            var quantity = await stockService.GetQuantityBySizeAsync(productId, selectedSize);
            if (quantity <= 0) {
                TempData["Error"] = "Товар недоступен в выбранном размере";
                return RedirectToPage("/Product", new { id = productId });
            }

            BasketShopping b = basketShopping.GetBasketShopping();
            b.Products.Add(new BasketItem { ProductId = productId, Size = selectedSize });
            basketShopping.SetBasketShopping(b);

            return RedirectToPage("/Product", new { id = productId });
        }
    }
}
