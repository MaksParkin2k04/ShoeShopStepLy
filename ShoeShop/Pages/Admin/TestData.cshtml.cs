using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;
using ShoeShop.Data;
using ShoeShop.Services;
using Microsoft.EntityFrameworkCore;

namespace ShoeShop.Pages.Admin {
    public class TestDataModel : PageModel {
        private readonly ApplicationContext _context;
        private readonly StockService _stockService;
        
        public TestDataModel(ApplicationContext context, StockService stockService) {
            _context = context;
            _stockService = stockService;
        }
        
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }
        public int CategoriesCount { get; set; }
        public int ProductsCount { get; set; }
        public List<string> Categories { get; set; } = new();
        
        public async Task OnGetAsync() {
            await LoadData();
        }
        
        public async Task<IActionResult> OnPostCreateCategoriesAsync() {
            try {
                var categories = new[] {
                    Category.Create("Мужская обувь"),
                    Category.Create("Женская обувь"),
                    Category.Create("Детская обувь")
                };
                
                foreach (var category in categories) {
                    if (!await _context.Categories.AnyAsync(c => c.Name == category.Name)) {
                        _context.Categories.Add(category);
                    }
                }
                
                await _context.SaveChangesAsync();
                Message = "Категории созданы!";
                IsSuccess = true;
            }
            catch (Exception ex) {
                Message = $"Ошибка: {ex.Message}";
                IsSuccess = false;
            }
            
            await LoadData();
            return Page();
        }
        
        public async Task<IActionResult> OnPostCreateProductsAsync() {
            try {
                var categories = await _context.Categories.ToListAsync();
                var menCategory = categories.FirstOrDefault(c => c.Name.Contains("Мужская"));
                var womenCategory = categories.FirstOrDefault(c => c.Name.Contains("Женская"));
                var kidsCategory = categories.FirstOrDefault(c => c.Name.Contains("Детская"));
                
                var products = new List<Product>();
                
                // Мужские кроссовки
                if (menCategory != null) {
                    var menShoes = new[] {
                        ("Nike Air Max 270", 12990, "Стильные мужские кроссовки Nike Air Max 270"),
                        ("Adidas Ultraboost 22", 15990, "Беговые кроссовки Adidas Ultraboost"),
                        ("Puma RS-X", 8990, "Ретро кроссовки Puma RS-X"),
                        ("New Balance 574", 7990, "Классические кроссовки New Balance"),
                        ("Reebok Classic Leather", 6990, "Кожаные кроссовки Reebok Classic")
                    };
                    
                    foreach (var (name, price, desc) in menShoes) {
                        var product = Product.Create(name, true, price, ProductSize.All, DateTime.Now, desc, desc, menCategory.Id);
                        products.Add(product);
                    }
                }
                
                // Женские кроссовки
                if (womenCategory != null) {
                    var womenShoes = new[] {
                        ("Nike Air Force 1", 11990, "Женские кроссовки Nike Air Force 1"),
                        ("Adidas Stan Smith", 8990, "Женские кеды Adidas Stan Smith"),
                        ("Puma Cali", 7990, "Женские кроссовки Puma Cali"),
                        ("New Balance 327", 9490, "Ретро кроссовки New Balance 327"),
                        ("Reebok Club C 85", 6990, "Женские кеды Reebok Club C")
                    };
                    
                    foreach (var (name, price, desc) in womenShoes) {
                        var product = Product.Create(name, true, price, ProductSize.All, DateTime.Now, desc, desc, womenCategory.Id);
                        products.Add(product);
                    }
                }
                
                // Детские кроссовки
                if (kidsCategory != null) {
                    var kidsShoes = new[] {
                        ("Nike Air Max Kids", 6990, "Детские кроссовки Nike Air Max"),
                        ("Adidas Superstar Kids", 5990, "Детские кеды Adidas Superstar"),
                        ("Puma Smash Kids", 3990, "Детские кеды Puma Smash"),
                        ("New Balance Kids", 4990, "Детские кроссовки New Balance"),
                        ("Skechers Lights", 4990, "Светящиеся кроссовки Skechers")
                    };
                    
                    foreach (var (name, price, desc) in kidsShoes) {
                        var product = Product.Create(name, true, price, ProductSize.From26To32, DateTime.Now, desc, desc, kidsCategory.Id);
                        products.Add(product);
                    }
                }
                
                _context.Products.AddRange(products);
                await _context.SaveChangesAsync();
                
                // Создаем остатки для всех товаров
                foreach (var product in products) {
                    var sizes = GetAvailableSizes(product.Sizes);
                    foreach (var size in sizes) {
                        var quantity = Random.Shared.Next(5, 25); // От 5 до 25 штук
                        var purchasePrice = product.Price * 0.6; // Закупочная цена 60% от продажной
                        await _stockService.SetStockAsync(product.Id, size, quantity, purchasePrice);
                    }
                }
                
                Message = $"Добавлено {products.Count} товаров с остатками!";
                IsSuccess = true;
            }
            catch (Exception ex) {
                Message = $"Ошибка: {ex.Message}";
                IsSuccess = false;
            }
            
            await LoadData();
            return Page();
        }
        
        public async Task<IActionResult> OnPostClearDataAsync() {
            try {
                var products = await _context.Products.ToListAsync();
                _context.Products.RemoveRange(products);
                await _context.SaveChangesAsync();
                
                Message = "Все товары удалены!";
                IsSuccess = true;
            }
            catch (Exception ex) {
                Message = $"Ошибка: {ex.Message}";
                IsSuccess = false;
            }
            
            await LoadData();
            return Page();
        }
        
        private async Task LoadData() {
            CategoriesCount = await _context.Categories.CountAsync();
            ProductsCount = await _context.Products.CountAsync();
            Categories = await _context.Categories.Select(c => c.Name).ToListAsync();
        }
        
        private List<int> GetAvailableSizes(ProductSize sizes) {
            var availableSizes = new List<int>();
            for (int size = 1; size <= 64; size++) {
                var sizeFlag = (ProductSize)(1UL << (size - 1));
                if (sizes.HasFlag(sizeFlag)) {
                    availableSizes.Add(size);
                }
            }
            return availableSizes;
        }
    }
}