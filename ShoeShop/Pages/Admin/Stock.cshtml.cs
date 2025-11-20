using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Attributes;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Pages.Admin {
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
        public int CurrentPage { get; private set; }
        public int ElementsPerPage { get; private set; }
        public int TotalElementsCount { get; private set; }

        public async Task OnGetAsync(string? search = null, string? status = null, int pageIndex = 1) {
            SearchQuery = search;
            StatusFilter = status;
            CurrentPage = pageIndex;
            ElementsPerPage = 5;
            
            // Только для выпадающих списков - минимум данных
            Products = await _productRepository.GetProducts(ProductSorting.Default, 0, 50);
            
            // Простой запрос остатков без JOIN
            ProductStocks = await _stockRepository.GetSimpleStocksAsync((pageIndex - 1) * ElementsPerPage, ElementsPerPage);
            TotalElementsCount = await _stockRepository.GetTotalStocksCountAsync();
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