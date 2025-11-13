namespace ShoeShop.Models {
    /// <summary>
    /// Представляет еденицу заказа 
    /// </summary>
    public class OrderDetail {
       
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="image">Изображение товара</param>
        /// <param name="name">Название товара</param>
        /// <param name="price">Цена товара</param>
        /// <param name="size">Размер товара</param>
        private OrderDetail(Guid id, Guid productId, string image, string name, double price, int size) {
            Id = id;
            ProductId = productId;
            Image = image;
            Name = name;
            Price = price;
            Size = size;
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
        /// Изображение товара
        /// </summary>
        public string Image { get; private set; }
        /// <summary>
        /// Название товара
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Цена товара
        /// </summary>
        public double Price { get; private set; }
        /// <summary>
        /// Размер товара
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Создает еденицу заказа
        /// </summary>
        /// <param name="productId">Идентификатор товара</param>
        /// <param name="image">Изображение товара</param>
        /// <param name="name">Название товара</param>
        /// <param name="price">Цена товара</param>
        /// <param name="size">Размер товара</param>
        /// <returns>Объект представляющий еденицу заказа</returns>
        public static OrderDetail Create(Guid productId, string image, string name, double price, int size) {
            return new OrderDetail(Guid.NewGuid(), productId, image, name, price, size);
        }
    }
}
