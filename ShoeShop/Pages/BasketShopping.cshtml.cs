using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages {
    public class BasketShoppingModel : PageModel {

        public BasketShoppingModel(IProductRepository repository, IBasketShoppingService basketShopping) {
            this.repository = repository;
            this.basketShopping = basketShopping;
        }

        private IProductRepository repository;
        private readonly IBasketShoppingService basketShopping;

        public IEnumerable<Product>? Products { get; private set; }

        public List<BasketProductInfo> BasketItems { get; private set; } = new List<BasketProductInfo>();

        public async Task OnGet() {
            BasketShopping bs = basketShopping.GetBasketShopping();

            List<BasketProductInfo> list = new List<BasketProductInfo>();
            Guid[] productIds = bs.Products.Select(p => p.ProductId).ToArray();
            Product[] products = (await repository.GetProducts(productIds)).ToArray();
            List<BasketItem> validItems = new List<BasketItem>();

            foreach (BasketItem item in bs.Products) {
                Product? product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null) {
                    list.Add(new BasketProductInfo { Product = product, Size = item.Size, Quantity = item.Quantity });
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

        public IActionResult OnPostUpdateQuantity(Guid productId, int size, int quantity) {
            if (quantity <= 0) return RedirectToPage("/BasketShopping");
            
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
    }
}
