using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using ShoeShop.Infrastructure;
using ShoeShop.Models;

namespace ShoeShop.Pages.Admin {

    [Authorize(Roles = "Admin")]
    public class AddProductModel : PageModel {

        public AddProductModel(IProductManager productManager) {
            this.productManager = productManager;
        }

        private readonly IProductManager productManager;

        public EditProduct Product { get; private set; }

        public void OnGet() {
            Product = new EditProduct();
        }

        public async Task<IActionResult> OnPostAsync(EditProduct product) {
            Guid addProductId = await productManager.Add(product);
            return RedirectToPage("/Admin/EditProduct", new { productId = addProductId });
        }
    }
}
