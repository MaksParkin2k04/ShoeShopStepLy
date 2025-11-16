namespace ShoeShop.Models {
    /// <summary>
    /// Репозиторий для работы с остатками товаров
    /// </summary>
    public interface IProductStockRepository {
        /// <summary>
        /// Получить остатки по товару
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <returns>Список остатков по размерам</returns>
        Task<IEnumerable<ProductStock>> GetByProductIdAsync(Guid productId);

        /// <summary>
        /// Получить остаток по товару и размеру
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="size">Размер</param>
        /// <returns>Остаток или null</returns>
        Task<ProductStock?> GetByProductAndSizeAsync(Guid productId, int size);

        /// <summary>
        /// Добавить или обновить остаток
        /// </summary>
        /// <param name="productStock">Остаток</param>
        Task SaveAsync(ProductStock productStock);

        /// <summary>
        /// Получить общее количество товара
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <returns>Общее количество</returns>
        Task<int> GetTotalQuantityAsync(Guid productId);

        /// <summary>
        /// Проверить наличие товара по размеру
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="size">Размер</param>
        /// <returns>Количество в наличии</returns>
        Task<int> GetQuantityAsync(Guid productId, int size);
    }
}