using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Data.Initialization {
    /// <summary>
    /// Инициализатор тестовых данных для остатков
    /// </summary>
    public static class StockInitializer {
        /// <summary>
        /// Инициализировать тестовые остатки
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="stockService">Сервис остатков</param>
        public static async Task InitializeAsync(ApplicationContext context, StockService stockService) {
            // Проверяем, есть ли уже остатки
            if (context.ProductStocks.Any()) {
                return; // Остатки уже инициализированы
            }

            // Получаем все товары
            var products = context.Products.ToList();

            var random = new Random();

            foreach (var product in products) {
                // Для каждого товара создаем остатки по размерам
                for (int size = 36; size <= 45; size++) {
                    var sizeFlag = (ProductSize)Enum.Parse(typeof(ProductSize), $"S{size}");
                    
                    // Проверяем, есть ли этот размер у товара
                    if (product.Sizes.HasFlag(sizeFlag)) {
                        // Генерируем случайное количество от 0 до 15
                        int quantity = random.Next(0, 16);
                        
                        if (quantity > 0) {
                            await stockService.AddStockAsync(product.Id, size, quantity);
                        }
                    }
                }
            }
        }
    }
}