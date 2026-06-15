namespace ShoeShop.Models {
    /// <summary>
    /// Изображение товара
    /// </summary>
    public class ProductImage {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="path">Путь к изображению</param>
        /// <param name="alt">Описание изображения</param>
        /// <param name="productId">Идентификатор товара</param>
        private ProductImage(Guid id, string path, string alt, Guid productId) {
            Id = id;
            Path = path;
            Alt = alt;
            ProductId = productId;
        }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Путь к изображению
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Описание изображения
        /// </summary>
        public string Alt { get; private set; }

        /// <summary>
        /// Идентификатор товара
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Устоновить путь к изображению
        /// </summary>
        /// <param name="path">Путь к изображению</param>
        public void SetPath(string path) {
            Path = path;
        }

        /// <summary>
        /// Устоновить описание изображения
        /// </summary>
        /// <param name="alt">Описание изображения</param>
        public void SetAlt(string alt) {
            Alt = alt;
        }

        /// <summary>
        /// Создает экземпляр объекта ProductImage
        /// </summary>
        /// <param name="path">Путь к изображению</param>
        /// <param name="alt">Описание изображения</param>
        /// <param name="productId">Идентификатор товара</param>
        /// <returns>Созданный объект</returns>
        public static ProductImage Create(string path, string alt, Guid productId) {
            return new ProductImage(Guid.NewGuid(), path, alt, productId);
        }
    }
}
