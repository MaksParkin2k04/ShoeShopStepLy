using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages {
    public class ProductModel : PageModel {
        public ProductModel(IProductRepository repository, IBasketShoppingService basketShopping) {
            this.repository = repository;
            this.basketShopping = basketShopping;
        }

        private IProductRepository repository;
        private readonly IBasketShoppingService basketShopping;

        public Product? Product { get; private set; }

        public async Task OnGetAsync(Guid id) {
            Product = await repository.GetProduct(id);
        }

        public IActionResult OnPost(Guid productId) {
            BasketShopping b = basketShopping.GetBasketShopping();
            b.Products.Add(productId);
            basketShopping.SetBasketShopping(b);

            return RedirectToPage("/Product", new { id = productId });
        }
    }
}
