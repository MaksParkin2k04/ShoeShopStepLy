using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.Attributes;
using ShoeShop.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.Services;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin {
    [Authorize]
    [AdminAuth]
    public class StockModel : PageModel {
        private readonly IProductRepository _productRepository;
        private readonly IProductStockRepository _stockRepository;
        private readonly StockService _stockService;

        public StockModel(IProductRepository productRepository, IProductStockRepository stockRepository, StockService stockService) {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _stockService = stockService;
        }

        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
        public string? Message { get; set; }
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }

        public async Task OnGetAsync(string? search = null, string? status = null) {
            SearchQuery = search;
            StatusFilter = status;
            
            Products = await _productRepository.GetProducts(ProductSorting.Default, 0, int.MaxValue);
            
            var allStocks = new List<ProductStock>();
            foreach (var product in Products) {
                var stocks = await _stockRepository.GetByProductIdAsync(product.Id);
                allStocks.AddRange(stocks);
            }
            
            // Применяем фильтры
            var filteredStocks = allStocks.AsEnumerable();
            
            if (!string.IsNullOrEmpty(search)) {
                filteredStocks = filteredStocks.Where(s => {
                    var product = Products.FirstOrDefault(p => p.Id == s.ProductId);
                    return product?.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;
                });
            }
            
            if (!string.IsNullOrEmpty(status)) {
                filteredStocks = status switch {
                    "outofstock" => filteredStocks.Where(s => s.Quantity == 0),
                    "lowstock" => filteredStocks.Where(s => s.Quantity > 0 && s.Quantity < 5),
                    "instock" => filteredStocks.Where(s => s.Quantity >= 5),
                    _ => filteredStocks
                };
            }
            
            ProductStocks = filteredStocks.OrderBy(s => {
                var product = Products.FirstOrDefault(p => p.Id == s.ProductId);
                return product?.Name ?? "";
            }).ThenBy(s => s.Size);
        }

        public async Task<IActionResult> OnPostAddStockAsync(Guid productId, int size, int quantity, double purchasePrice = 0) {
            try {
                await _stockService.AddStockAsync(productId, size, quantity, purchasePrice);
                Message = $"Успешно добавлен приход: {quantity} пар размера {size}";
            } catch (Exception ex) {
                Message = $"Ошибка при добавлении прихода: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReduceStockAsync(Guid productId, int size, int quantity) {
            try {
                await _stockService.ReduceStockAsync(productId, size, quantity);
                Message = $"Успешно списано: {quantity} пар размера {size}";
            } catch (Exception ex) {
                Message = $"Ошибка при списании: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdatePriceAsync(Guid productId, int size, double purchasePrice) {
            try {
                await _stockService.UpdatePurchasePriceAsync(productId, size, purchasePrice);
                Message = $"Цена закупки обновлена: {purchasePrice:F2} ₽";
            } catch (Exception ex) {
                Message = $"Ошибка при обновлении цены: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
