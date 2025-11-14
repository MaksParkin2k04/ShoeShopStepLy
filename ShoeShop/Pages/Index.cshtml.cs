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
        public Guid? CategoryId { get; private set; }
        public decimal? MinPrice { get; private set; }
        public decimal? MaxPrice { get; private set; }
        public int[]? Sizes { get; private set; }
        public IEnumerable<Product>? Products { get; private set; }

        public async Task OnGetAsync(ProductSorting sort = ProductSorting.Default, int pageIndex = 1, Guid? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null, int[]? sizes = null) {
            Sorting = sort;
            CurrentPage = pageIndex;
            ElementsPerPage = 20;
            CategoryId = categoryId;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            Sizes = sizes;
            
            var allProducts = await repository.GetProducts(sort, 0, int.MaxValue);
            
            // Применяем фильтры
            var filteredProducts = allProducts.AsEnumerable();
            
            if (categoryId.HasValue) {
                filteredProducts = filteredProducts.Where(p => p.CategoryId == categoryId.Value);
            }
            
            if (minPrice.HasValue) {
                filteredProducts = filteredProducts.Where(p => p.Price >= (double)minPrice.Value);
            }
            
            if (maxPrice.HasValue) {
                filteredProducts = filteredProducts.Where(p => p.Price <= (double)maxPrice.Value);
            }
            
            if (sizes != null && sizes.Length > 0) {
                filteredProducts = filteredProducts.Where(p => {
                    return sizes.Any(size => {
                        var sizeFlag = (ProductSize)Enum.Parse(typeof(ProductSize), $"S{size}");
                        return p.Sizes.HasFlag(sizeFlag);
                    });
                });
            }
            
            TotalElementsCount = filteredProducts.Count();
            Products = filteredProducts.Skip((pageIndex - 1) * 20).Take(20);
        }
    }
}
