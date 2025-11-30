using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Data.Initialization {
    public static class StockInitializer {
        public static async Task InitializeAsync(ApplicationContext context, StockService stockService) {
            // Проверяем, есть ли уже остатки
            if (context.ProductStocks.Any()) {
                return; // Остатки уже инициализированы
            }

            // Получаем все товары
            var products = context.Products.ToList();
            
            foreach (var product in products) {
                // Получаем доступные размеры для товара
                var availableSizes = GetAvailableSizes(product.Sizes);
                
                foreach (var size in availableSizes) {
                    // Создаем случайные остатки от 0 до 20
                    var random = new Random();
                    var quantity = random.Next(0, 21);
                    var purchasePrice = product.Price * 0.6; // 60% от розничной цены
                    
                    await stockService.SetStockAsync(product.Id, size, quantity, purchasePrice);
                }
            }
        }
        
        private static List<int> GetAvailableSizes(ProductSize sizes) {
            var availableSizes = new List<int>();
            
            for (int size = 35; size <= 46; size++) {
                var sizeFlag = (ProductSize)(1UL << (size - 1));
                if (sizes.HasFlag(sizeFlag)) {
                    availableSizes.Add(size);
                }
            }
            
            return availableSizes.Any() ? availableSizes : new List<int> { 40, 41, 42, 43 };
        }
    }
}