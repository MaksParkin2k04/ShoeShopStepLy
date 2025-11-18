using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages
{
    [Authorize(Roles = "Admin")]
    public class ProductSizeModel : PageModel
    {
        private readonly IProductRepository repository;
        private readonly IProductStockRepository stockRepository;

        public ProductSizeModel(IProductRepository repository, IProductStockRepository stockRepository)
        {
            this.repository = repository;
            this.stockRepository = stockRepository;
        }

        public Product? Product { get; set; }
        public int Size { get; set; }
        public int StockQuantity { get; set; }
        public bool IsSizeAvailable { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid productId, int size)
        {
            Product = await repository.GetProduct(productId);
            Size = size;
            
            if (Product == null)
            {
                return NotFound();
            }

            // Проверяем доступность размера
            var sizeFlag = (ProductSize)(1UL << (size - 1));
            IsSizeAvailable = Product.Sizes.HasFlag(sizeFlag);
            
            if (!IsSizeAvailable)
            {
                return NotFound("Размер недоступен");
            }

            // Получаем реальное количество на складе
            StockQuantity = await stockRepository.GetQuantityAsync(productId, size);

            return Page();
        }
    }
}