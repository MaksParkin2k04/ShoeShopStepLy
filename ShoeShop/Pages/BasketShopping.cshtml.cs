using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages {
    public class BasketShoppingModel : PageModel {

        public BasketShoppingModel(IProductRepository repository, IBasketShoppingService basketShopping, IProductStockRepository stockRepository) {
            this.repository = repository;
            this.basketShopping = basketShopping;
            this.stockRepository = stockRepository;
        }

        private IProductRepository repository;
        private readonly IBasketShoppingService basketShopping;
        private readonly IProductStockRepository stockRepository;

        public IEnumerable<Product>? Products { get; private set; }

        public List<BasketProductInfo> BasketItems { get; private set; } = new List<BasketProductInfo>();
        
        public bool HasStockIssues => BasketItems.Any(item => item.AvailableStock < item.Quantity);

        public async Task OnGet() {
            BasketShopping bs = basketShopping.GetBasketShopping();

            List<BasketProductInfo> list = new List<BasketProductInfo>();
            Guid[] productIds = bs.Products.Select(p => p.ProductId).ToArray();
            Product[] products = (await repository.GetProducts(productIds)).ToArray();
            List<BasketItem> validItems = new List<BasketItem>();

            foreach (BasketItem item in bs.Products) {
                Product? product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null) {
                    var stock = await stockRepository.GetByProductAndSizeAsync(item.ProductId, item.Size);
                    var availableStock = stock?.Quantity ?? 0;
                    
                    list.Add(new BasketProductInfo { 
                        Product = product, 
                        Size = item.Size, 
                        Quantity = item.Quantity,
                        AvailableStock = availableStock
                    });
                    validItems.Add(item);
                }
            }

            // Обновляем корзину, удаляя несуществующие товары
            if (validItems.Count != bs.Products.Count) {
                bs.Products.Clear();
                bs.Products.AddRange(validItems);
                basketShopping.SetBasketShopping(bs);
            }

            BasketItems = list;
        }

        public void OnPost() {
        }

        public IActionResult OnPostDeleteProduct(Guid productId, int size) {
            BasketShopping bs = basketShopping.GetBasketShopping();
            BasketItem? item = bs.Products.FirstOrDefault(p => p.ProductId == productId && p.Size == size);
            if (item != null) {
                bs.Products.Remove(item);
                basketShopping.SetBasketShopping(bs);
            }

            return RedirectToPage("/BasketShopping");
        }

        public async Task<IActionResult> OnPostUpdateQuantity(Guid productId, int size, int quantity) {
            if (quantity <= 0) return RedirectToPage("/BasketShopping");
            
            // Проверяем наличие на складе
            var stock = await stockRepository.GetByProductAndSizeAsync(productId, size);
            var availableStock = stock?.Quantity ?? 0;
            
            if (quantity > availableStock) {
                TempData["Error"] = $"Доступно только {availableStock} шт. данного размера";
                return RedirectToPage("/BasketShopping");
            }
            
            BasketShopping bs = basketShopping.GetBasketShopping();
            BasketItem? item = bs.Products.FirstOrDefault(p => p.ProductId == productId && p.Size == size);
            if (item != null) {
                item.Quantity = quantity;
                basketShopping.SetBasketShopping(bs);
            }

            return RedirectToPage("/BasketShopping");
        }
    }

    public class BasketProductInfo {
        public Product Product { get; set; }
        public int Size { get; set; }
        public int Quantity { get; set; } = 1;
        public int AvailableStock { get; set; }
    }
}
