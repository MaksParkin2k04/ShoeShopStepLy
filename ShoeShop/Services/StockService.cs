using ShoeShop.Models;

namespace ShoeShop.Services {
    /// <summary>
    /// Сервис для управления остатками товаров
    /// </summary>
    public class StockService {
        private readonly IProductStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;

        public StockService(IProductStockRepository stockRepository, IProductRepository productRepository) {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }

        /// <summary>
        /// Добавить приход товара
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="size">Размер</param>
        /// <param name="quantity">Количество</param>
        /// <param name="purchasePrice">Цена закупки</param>
        public async Task SetStockAsync(Guid productId, int size, int quantity, double purchasePrice = 0) {
            var existing = await _stockRepository.GetByProductAndSizeAsync(productId, size);
            
            if (existing == null) {
                var newStock = ProductStock.Create(productId, size, quantity, purchasePrice);
                await _stockRepository.SaveAsync(newStock);
            } else {
                existing.SetQuantity(quantity);
                existing.SetPurchasePrice(purchasePrice);
                await _stockRepository.SaveAsync(existing);
            }
        }

        public async Task AddStockAsync(Guid productId, int size, int quantity, double purchasePrice = 0) {
            var existing = await _stockRepository.GetByProductAndSizeAsync(productId, size);
            
            if (existing == null) {
                var newStock = ProductStock.Create(productId, size, quantity, purchasePrice);
                await _stockRepository.SaveAsync(newStock);
            } else {
                existing.AddQuantity(quantity);
                await _stockRepository.SaveAsync(existing);
            }
        }

        /// <summary>
        /// Проверить статус наличия товара
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <returns>Статус наличия</returns>
        public async Task<ProductAvailabilityStatus> GetAvailabilityStatusAsync(Guid productId) {
            var product = await _productRepository.GetProduct(productId);
            if (product == null) return ProductAvailabilityStatus.OutOfStock;
            
            var stocks = await _stockRepository.GetByProductIdAsync(productId);
            var totalQuantity = stocks.Where(s => IsValidSize(product, s.Size)).Sum(s => s.Quantity);
            
            if (totalQuantity == 0) {
                return ProductAvailabilityStatus.OutOfStock;
            }
            
            if (totalQuantity < 5) {
                return ProductAvailabilityStatus.LowStock;
            }
            
            return ProductAvailabilityStatus.InStock;
        }

        /// <summary>
        /// Получить количество по размеру
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="size">Размер</param>
        /// <returns>Количество</returns>
        public async Task<int> GetQuantityBySizeAsync(Guid productId, int size) {
            var product = await _productRepository.GetProduct(productId);
            if (product == null || !IsValidSize(product, size)) return 0;
            
            return await _stockRepository.GetQuantityAsync(productId, size);
        }

        /// <summary>
        /// Получить информацию о наличии по размерам
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <returns>Словарь размер-количество</returns>
        public async Task<Dictionary<int, int>> GetSizeQuantitiesAsync(Guid productId) {
            var product = await _productRepository.GetProduct(productId);
            if (product == null) return new Dictionary<int, int>();
            
            var stocks = await _stockRepository.GetByProductIdAsync(productId);
            return stocks.Where(s => IsValidSize(product, s.Size)).ToDictionary(s => s.Size, s => s.Quantity);
        }

        /// <summary>
        /// Уменьшить количество товара при покупке
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="size">Размер</param>
        /// <param name="quantity">Количество для уменьшения</param>
        public async Task ReduceStockAsync(Guid productId, int size, int quantity = 1) {
            var stock = await _stockRepository.GetByProductAndSizeAsync(productId, size);
            if (stock == null) {
                throw new InvalidOperationException($"Остатки по размеру {size} не найдены");
            }
            if (stock.Quantity < quantity) {
                throw new InvalidOperationException($"Недостаточно товара. Доступно: {stock.Quantity}, запрошено: {quantity}");
            }
            stock.ReduceQuantity(quantity);
            await _stockRepository.SaveAsync(stock);
        }

        /// <summary>
        /// Уменьшить количество товара при покупке (безопасно)
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="size">Размер</param>
        /// <param name="quantity">Количество</param>
        public async Task ReduceStockSafeAsync(Guid productId, int size, int quantity = 1) {
            var stock = await _stockRepository.GetByProductAndSizeAsync(productId, size);
            if (stock != null && stock.Quantity >= quantity) {
                stock.ReduceQuantity(quantity);
                await _stockRepository.SaveAsync(stock);
            }
        }

        /// <summary>
        /// Обновить цену закупки
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="size">Размер</param>
        /// <param name="purchasePrice">Новая цена закупки</param>
        public async Task UpdatePurchasePriceAsync(Guid productId, int size, double purchasePrice) {
            var stock = await _stockRepository.GetByProductAndSizeAsync(productId, size);
            if (stock != null) {
                stock.SetPurchasePrice(purchasePrice);
                await _stockRepository.SaveAsync(stock);
            }
        }

        /// <summary>
        /// Проверить наличие нужного количества товара
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="size">Размер</param>
        /// <param name="requiredQuantity">Требуемое количество</param>
        /// <returns>true если товара достаточно</returns>
        public async Task<bool> IsAvailableAsync(Guid productId, int size, int requiredQuantity = 1) {
            var availableQuantity = await GetQuantityBySizeAsync(productId, size);
            return availableQuantity >= requiredQuantity;
        }



        /// <summary>
        /// Проверить, что размер указан в карточке товара
        /// </summary>
        /// <param name="product">Товар</param>
        /// <param name="size">Размер</param>
        /// <returns>true если размер валиден</returns>
        private bool IsValidSize(Product product, int size) {
            if (size < 1 || size > 64) return false;
            
            var sizeFlag = (ProductSize)(1UL << (size - 1));
            return product.Sizes.HasFlag(sizeFlag);
        }
    }

    /// <summary>
    /// Статус наличия товара
    /// </summary>
    public enum ProductAvailabilityStatus {
        /// <summary>
        /// В наличии
        /// </summary>
        InStock,
        /// <summary>
        /// Мало товара
        /// </summary>
        LowStock,
        /// <summary>
        /// Нет в наличии
        /// </summary>
        OutOfStock
    }
}