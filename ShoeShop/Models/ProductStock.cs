namespace ShoeShop.Models {
    /// <summary>
    /// Остатки товара по размерам
    /// </summary>
    public class ProductStock {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="size">Размер</param>
        /// <param name="quantity">Количество</param>
        private ProductStock(Guid id, Guid productId, int size, int quantity) {
            Id = id;
            ProductId = productId;
            Size = size;
            Quantity = quantity;
        }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Идентификатор товара
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Размер
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Количество
        /// </summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// Товар
        /// </summary>
        public Product? Product { get; private set; }

        /// <summary>
        /// Добавить количество
        /// </summary>
        /// <param name="amount">Количество для добавления</param>
        public void AddQuantity(int amount) {
            if (amount < 0) throw new ArgumentException("Количество не может быть отрицательным");
            Quantity += amount;
        }

        /// <summary>
        /// Уменьшить количество
        /// </summary>
        /// <param name="amount">Количество для уменьшения</param>
        public void ReduceQuantity(int amount) {
            if (amount < 0) throw new ArgumentException("Количество не может быть отрицательным");
            if (Quantity < amount) throw new InvalidOperationException("Недостаточно товара на складе");
            Quantity -= amount;
        }

        /// <summary>
        /// Установить количество
        /// </summary>
        /// <param name="quantity">Новое количество</param>
        public void SetQuantity(int quantity) {
            if (quantity < 0) throw new ArgumentException("Количество не может быть отрицательным");
            Quantity = quantity;
        }

        /// <summary>
        /// Создать запись остатков
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="size">Размер</param>
        /// <param name="quantity">Количество</param>
        /// <returns>Новая запись остатков</returns>
        public static ProductStock Create(Guid productId, int size, int quantity) {
            return new ProductStock(Guid.NewGuid(), productId, size, quantity);
        }
    }
}