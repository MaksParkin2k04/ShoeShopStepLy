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

        public async Task OnGet() {
            BasketShopping bs = basketShopping.GetBasketShopping();

            List<Product> list = new List<Product>();
            Product[] products = (await repository.GetProducts(bs.Products.ToArray())).ToArray();

            foreach (Guid id in bs.Products) {
                list.Add(products.First(p => p.Id == id));
            }

            Products = list.ToArray();
        }

        public void OnPost() {
        }

        public IActionResult OnPostDeleteProduct(Guid productId) {
            BasketShopping bs = basketShopping.GetBasketShopping();
            bs.Products.Remove(productId);
            basketShopping.SetBasketShopping(bs);

            return RedirectToPage("/BasketShopping");
        }
    }
}
