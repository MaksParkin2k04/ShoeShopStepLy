using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.Infrastructure;
using ShoeShop.MultiTenantAdmin.Models;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin {

    [Authorize(Roles = "Admin")]
    public class EditProductModel : PageModel {

        public EditProductModel(IAdminRepository repository, IProductManager productManager) {
            this.repository = repository;
            this.productManager = productManager;
        }

        private readonly IAdminRepository repository;
        private readonly IProductManager productManager;

        public Product? Product { get; private set; }
        public IEnumerable<Category> Categories { get; private set; }

        public async Task OnGetAsync(Guid productId) {
            Product = await repository.GetProduct(productId);
            Categories = await repository.GetCategories();
        }

        public async Task<IActionResult> OnPostAsync(EditProduct product, ulong[] sizes, double? saleprice) {
            if (sizes != null && sizes.Length > 0) {
                ProductSize combinedSizes = ProductSize.Not;
                foreach (ulong size in sizes) {
                    combinedSizes |= (ProductSize)size;
                }
                product.Sizes = combinedSizes;
            }
            
            product.SalePrice = saleprice;
            
            Guid productId = await productManager.Update(product);
            return RedirectToPage("/Admin/EditProduct", new { productId = productId });
        }

        public IEnumerable<string> GetSizes(ProductSize shoeSize) {
            foreach (ProductSize size in Enum.GetValues(typeof(ProductSize))) {
                if (size != ProductSize.Not && shoeSize.HasFlag(size)) {
                    string name = Enum.GetName<ProductSize>(size);
                    name = name.Substring(1);
                   if( double.TryParse(name, out double value)) {
                        yield return (value / 10).ToString();
                    }

                    
                }
            }
        }
    }
}
