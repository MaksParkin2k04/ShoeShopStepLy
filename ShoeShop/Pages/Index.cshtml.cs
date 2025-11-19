using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Pages {
    public class IndexModel : PageModel {
        public IndexModel(IProductRepository repository, StockService stockService, ReviewService reviewService) {
            this.repository = repository;
            this.stockService = stockService;
            this.reviewService = reviewService;
        }

        private IProductRepository repository;
        private StockService stockService;
        private ReviewService reviewService;

        public int CurrentPage { get; private set; }
        public int ElementsPerPage { get; private set; }
        public int TotalElementsCount { get; private set; }
        public ProductSorting Sorting { get; private set; }
        public Guid? CategoryId { get; private set; }
        public decimal? MinPrice { get; private set; }
        public decimal? MaxPrice { get; private set; }
        public int[]? Sizes { get; private set; }
        public IEnumerable<Product>? Products { get; private set; }
        public Dictionary<Guid, ProductAvailabilityStatus> ProductAvailability { get; private set; } = new();
        public Dictionary<Guid, Dictionary<int, int>> ProductSizeQuantities { get; private set; } = new();
        public Dictionary<Guid, double> ProductRatings { get; private set; } = new();
        public Dictionary<Guid, int> ProductReviewCounts { get; private set; } = new();

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
            
            // Загружаем информацию о наличии для всех отфильтрованных товаров
            var productsWithAvailability = new List<(Product product, ProductAvailabilityStatus availability)>();
            foreach (var product in filteredProducts) {
                var availability = await stockService.GetAvailabilityStatusAsync(product.Id);
                productsWithAvailability.Add((product, availability));
            }
            
            // Сортируем: сначала в наличии, потом нет в наличии
            var sortedProducts = productsWithAvailability
                .OrderByDescending(x => x.availability == ProductAvailabilityStatus.InStock)
                .ThenBy(x => x.product.Name)
                .Select(x => x.product);
            
            TotalElementsCount = sortedProducts.Count();
            Products = sortedProducts.Skip((pageIndex - 1) * 20).Take(20);
            
            // Загружаем дополнительную информацию для отображаемых товаров
            foreach (var product in Products) {
                ProductAvailability[product.Id] = await stockService.GetAvailabilityStatusAsync(product.Id);
                ProductSizeQuantities[product.Id] = await stockService.GetSizeQuantitiesAsync(product.Id);
                ProductRatings[product.Id] = await reviewService.GetAverageRatingAsync(product.Id);
                ProductReviewCounts[product.Id] = await reviewService.GetReviewCountAsync(product.Id);
            }
        }
    }
}
