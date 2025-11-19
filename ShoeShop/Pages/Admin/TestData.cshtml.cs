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
                var brands = new[] { "Nike", "Adidas", "Puma", "New Balance", "Reebok", "Asics", "Converse", "Vans", "Skechers", "Under Armour" };
                var models = new[] { "Air Max", "Boost", "Classic", "Sport", "Run", "Flex", "Pro", "Elite", "Speed", "Comfort" };
                var colors = new[] { "Черные", "Белые", "Красные", "Синие", "Серые", "Зеленые", "Розовые", "Желтые" };
                
                for (int i = 0; i < 100; i++) {
                    var brand = brands[Random.Shared.Next(brands.Length)];
                    var model = models[Random.Shared.Next(models.Length)];
                    var color = colors[Random.Shared.Next(colors.Length)];
                    var number = Random.Shared.Next(100, 999);
                    
                    var name = $"{brand} {model} {number} {color}";
                    var price = Random.Shared.Next(3000, 25000);
                    var desc = $"Кроссовки {name} - стильная и комфортная обувь для повседневной носки";
                    
                    Guid categoryId;
                    ProductSize sizes;
                    
                    var categoryType = i % 3;
                    if (categoryType == 0 && menCategory != null) {
                        categoryId = menCategory.Id;
                        sizes = ProductSize.All;
                    } else if (categoryType == 1 && womenCategory != null) {
                        categoryId = womenCategory.Id;
                        sizes = ProductSize.All;
                    } else if (kidsCategory != null) {
                        categoryId = kidsCategory.Id;
                        sizes = ProductSize.From26To32;
                    } else {
                        continue;
                    }
                    
                    var product = Product.Create(name, true, price, sizes, DateTime.Now, desc, desc, categoryId);
                    products.Add(product);
                }
                
                _context.Products.AddRange(products);
                await _context.SaveChangesAsync();
                
                // Получаем все доступные изображения
                var allImageFiles = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products"), "*.jpg")
                    .Select(f => Path.GetFileName(f))
                    .ToArray();
                
                // Добавляем изображения к товарам
                foreach (var product in products) {
                    var imageCount = Random.Shared.Next(4, Math.Min(8, allImageFiles.Length + 1));
                    var selectedImages = allImageFiles.OrderBy(x => Random.Shared.Next()).Take(imageCount);
                    
                    foreach (var imageFile in selectedImages) {
                        var imagePath = $"/images/products/{imageFile}";
                        
                        // Устанавливаем ProductId через SQL
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO ProductImages (Id, Path, Alt, ProductId) VALUES ({0}, {1}, {2}, {3})",
                            Guid.NewGuid(), imagePath, $"Изображение {product.Name}", product.Id);
                    }
                }
                
                // Создаем остатки для всех товаров
                foreach (var product in products) {
                    var sizes = GetAvailableSizes(product.Sizes);
                    foreach (var size in sizes) {
                        var quantity = Random.Shared.Next(0, 15);
                        var purchasePrice = product.Price * 0.6;
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
                // Удаляем связанные данные сначала
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProductStocks");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProductImages");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProductReviews");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM OrderDetails");
                
                // Теперь удаляем товары
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Products");
                
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