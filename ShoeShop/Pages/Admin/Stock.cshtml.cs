using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Attributes;
using ShoeShop.Models;
using ShoeShop.Services;
using ShoeShop.Infrastructure;

namespace ShoeShop.Pages.Admin {
    [Authorize]
    [AdminAuth]
    public class StockModel : PageModel {
        private readonly IProductRepository _productRepository;
        private readonly IProductStockRepository _stockRepository;
        private readonly StockService _stockService;
        private readonly IAdminRepository _adminRepository;
        private readonly IProductManager _productManager;

        public StockModel(IProductRepository productRepository, IProductStockRepository stockRepository, StockService stockService, IAdminRepository adminRepository, IProductManager productManager) {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _stockService = stockService;
            _adminRepository = adminRepository;
            _productManager = productManager;
        }

        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
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
            ElementsPerPage = 20;
            
            // Получаем все товары для выпадающих списков
            Products = await _productRepository.GetProducts(ProductSorting.Default, 0, 1000);
            Categories = await _adminRepository.GetCategories();
            
            // Получаем остатки с поиском и фильтрацией
            if (!string.IsNullOrEmpty(search) || !string.IsNullOrEmpty(status)) {
                ProductStocks = await _stockRepository.GetStocksWithSearchAsync(search, status, (pageIndex - 1) * ElementsPerPage, ElementsPerPage);
                TotalElementsCount = await _stockRepository.GetStocksCountWithSearchAsync(search, status);
            } else {
                ProductStocks = await _stockRepository.GetSimpleStocksAsync((pageIndex - 1) * ElementsPerPage, ElementsPerPage);
                TotalElementsCount = await _stockRepository.GetTotalStocksCountAsync();
            }
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

        public async Task<IActionResult> OnPostCreateProductAsync(string name, bool isSale, double price, string description, string content, Guid categoryId, ulong[]? sizes) {
            try {
                var editProduct = new EditProduct {
                    Name = name,
                    IsSale = isSale,
                    Price = price,
                    Description = description,
                    Content = content,
                    CategoryId = categoryId,
                    Sizes = ProductSize.Not
                };

                if (sizes != null && sizes.Length > 0) {
                    ProductSize combinedSizes = ProductSize.Not;
                    foreach (ulong size in sizes) {
                        combinedSizes |= (ProductSize)size;
                    }
                    editProduct.Sizes = combinedSizes;
                }

                Guid productId = await _productManager.Add(editProduct);
                return RedirectToPage("/Admin/EditProduct", new { productId = productId });
            } catch (Exception ex) {
                Message = $"Ошибка при создании товара: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}