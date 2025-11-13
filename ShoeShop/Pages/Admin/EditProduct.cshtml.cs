using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Infrastructure;
using ShoeShop.Models;

namespace ShoeShop.Pages.Admin {

    [Authorize(Roles = "Admin")]
    public class EditProductModel : PageModel {

        public EditProductModel(IAdminRepository repository, IProductManager productManager) {
            this.repository = repository;
            this.productManager = productManager;
        }

        private readonly IAdminRepository repository;
        private readonly IProductManager productManager;

        public Product? Product { get; private set; }

        public async Task OnGetAsync(Guid productId) {
            Product = await repository.GetProduct(productId);
            string[] sizes = GetSizes(Product.Sizes).ToArray();
            string ss = string.Join(", ", sizes);
        }

        public async Task<IActionResult> OnPostAsync(EditProduct product) {
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
