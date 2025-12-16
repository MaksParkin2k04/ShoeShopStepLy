using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class StockManagementModel : PageModel
    {
        private readonly IProductRepository productRepository;
        private readonly IProductStockRepository stockRepository;
        private readonly StockService stockService;

        public StockManagementModel(IProductRepository productRepository, IProductStockRepository stockRepository, StockService stockService)
        {
            this.productRepository = productRepository;
            this.stockRepository = stockRepository;
            this.stockService = stockService;
        }

        public List<Product> Products { get; set; } = new();
        public List<ProductStock> CurrentStocks { get; set; } = new();

        public async Task OnGetAsync()
        {
            Products = (await productRepository.GetAllAsync()).ToList();
            CurrentStocks = (await stockRepository.GetByProductIdAsync(Guid.Empty)).ToList();
            
            // Получаем все остатки
            var allStocks = new List<ProductStock>();
            foreach (var product in Products)
            {
                var stocks = await stockRepository.GetByProductIdAsync(product.Id);
                allStocks.AddRange(stocks);
            }
            CurrentStocks = allStocks;
        }

        public async Task<IActionResult> OnPostAsync(Guid productId, int size, int quantity, double purchasePrice = 0)
        {
            await stockService.SetStockAsync(productId, size, quantity, purchasePrice);
            return RedirectToPage();
        }
    }
}