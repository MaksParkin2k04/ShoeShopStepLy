using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages {
    public class IndexModel : PageModel {
        public IndexModel(IProductRepository repository) {
            this.repository = repository;
        }

        private IProductRepository repository;

        public int CurrentPage { get; private set; }
        public int ElementsPerPage { get; private set; }
        public int TotalElementsCount { get; private set; }
        public ProductSorting Sorting { get; private set; }
        public IEnumerable<Product>? Products { get; private set; }

        public async Task OnGetAsync(ProductSorting sort, int pageIndex = 1) {
            Sorting = sort;
            CurrentPage = pageIndex;
            ElementsPerPage = 20;
            Products = await repository.GetProducts(sort, pageIndex - 1, 20);
            TotalElementsCount = await repository.ProductCount();
        }
    }
}
