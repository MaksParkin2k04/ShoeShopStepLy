using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages.Admin {

    [Authorize(Roles = "Admin")]
    public class DeleteProductModel : PageModel {

        public DeleteProductModel(IAdminRepository repository, IProductManager productManager) {
            this.repository = repository;
            this.productManager = productManager;
        }

        private readonly IAdminRepository repository;
        private readonly IProductManager productManager;

        public Product? Product { get; private set; }

        public async Task OnGetAsync(Guid productId) {
            Product = await repository.GetProduct(productId);
        }

        public async Task<IActionResult> OnPostAsync(Guid productId) {
            await productManager.Delete(productId);
            return RedirectToPage("Catalog");
        }
    }
}
