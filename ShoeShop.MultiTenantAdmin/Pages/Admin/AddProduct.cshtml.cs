using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using ShoeShop.MultiTenantAdmin.Infrastructure;
using ShoeShop.MultiTenantAdmin.Models;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin {

    [Authorize(Roles = "Admin")]
    public class AddProductModel : PageModel {

        public AddProductModel(IProductManager productManager, IAdminRepository adminRepository) {
            this.productManager = productManager;
            this.adminRepository = adminRepository;
        }

        private readonly IProductManager productManager;
        private readonly IAdminRepository adminRepository;

        public EditProduct Product { get; private set; }
        public IEnumerable<Category> Categories { get; private set; }

        public async Task OnGet() {
            Product = new EditProduct() {
                Sizes = ProductSize.Not
            };
            Categories = await adminRepository.GetCategories();
        }

        public async Task<IActionResult> OnPostAsync(EditProduct product, ulong[] sizes) {
            if (sizes != null && sizes.Length > 0) {
                ProductSize combinedSizes = ProductSize.Not;
                foreach (ulong size in sizes) {
                    combinedSizes |= (ProductSize)size;
                }
                product.Sizes = combinedSizes;
            }
            
            Guid addProductId = await productManager.Add(product);
            return RedirectToPage("/Admin/EditProduct", new { productId = addProductId });
        }
    }
}
