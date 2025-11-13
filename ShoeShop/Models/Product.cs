namespace ShoeShop.Models {
    /// <summary>
    /// Товар
    /// </summary>
    public class Product {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id">Идентификатор товара</param>
        /// <param name="name">Название товара</param>
        /// <param name="isSale">Если true товар продается, false товар снят с продажи</param>
        /// <param name="price">Цена товара</param>
        /// <param name="sizes">Доступные размеры товара</param>
        /// <param name="dateAdded">Дата добавления</param>
        /// <param name="description">Краткое описание товара</param>
        /// <param name="content">Описание товара</param>
        private Product(Guid id, string name, bool isSale, double price, ProductSize sizes, DateTime dateAdded, string description, string content) {
            Id = id;
            Name = name;
            IsSale = isSale;
            Price = price;
            Sizes = sizes;
            DateAdded = dateAdded;
            Description = description;
            Content = content;
            images = new List<ProductImage>();
        }

        private ProductSize sizes;
        private readonly List<ProductImage> images;

        /// <summary>
        /// Идентификатор товара
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Название товара
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Товар продается
        /// </summary>
        public bool IsSale { get; private set; }
        /// <summary>
        /// Цена товара
        /// </summary>
        public double Price { get; private set; }
        /// <summary>
        /// Доступные размеры товара
        /// </summary>
        public ProductSize Sizes { get; private set; }
        /// <summary>
        /// Дата добавления
        /// </summary>
        public DateTime DateAdded { get; private set; }
        /// <summary>
        /// Краткое описание товара.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Описание товара.
        /// </summary>
        public string Content { get; private set; }
        /// <summary>
        /// Изображения товара
        /// </summary>
        public IReadOnlyList<ProductImage> Images { get { return images; } }

        /// <summary>
        /// Устоновить название
        /// </summary>
        /// <param name="name">Название</param>
        public void SetName(string name) {
            Name = name;
        }

        /// <summary>
        /// Устоновить продается товар или нет
        /// </summary>
        /// <param name="isSale">Если true товар продается, false товар снят с продажи</param>
        public void SetIsSale(bool isSale) {
            IsSale = isSale;
        }

        /// <summary>
        /// Устоновить цену
        /// </summary>
        /// <param name="price">Цена</param>
        public void SetPrice(double price) {
            Price = price;
        }

        /// <summary>
        /// Устоновить доступные размеры товара
        /// </summary>
        /// <param name="shoeSize">Доступные размеры товара</param>
        public void SetSizes(ProductSize size) {
            Sizes = size;
            if (size == ProductSize.Not) {
                IsSale = false;
            }
        }

        /// <summary>
        /// Устоновить краткое описание
        /// </summary>
        /// <param name="description">Описание</param>
        public void SetDescription(string description) {
            Description = description;
        }

        /// <summary>
        /// Устоновить описание
        /// </summary>
        /// <param name="content">Описание</param>
        public void SetContent(string content) {
            Content = content;
        }

        /// <summary>
        /// Добавить изображение
        /// </summary>
        /// <param name="path">Путь к изображению</param>
        /// <param name="alt">Описание изображения</param>
        public void AddImage(string path, string alt) {
            ProductImage image = ProductImage.Create(path, alt);
            images.Add(image);
        }

        /// <summary>
        /// Обновить описание изображения
        /// </summary>
        /// <param name="imageId">Идентификатор изображения</param>
        /// <param name="alt">Описание изображения</param>
        /// <exception cref="ArgumentOutOfRangeException">Изображение отсутствует</exception>
        public void UpdateImageAlt(Guid imageId, string alt) {
            ProductImage? productImage = images.FirstOrDefault(i => i.Id == imageId);
            if (productImage == null) { throw new ArgumentOutOfRangeException(nameof(imageId), "Изображение отсутствует"); }

            productImage.SetAlt(alt);
        }

        /// <summary>
        /// Обновить изображение
        /// </summary>
        /// <param name="imageId">Идентификатор изображения</param>
        /// <param name="path">Путь к изображению</param>
        /// <param name="alt">Описание изображения</param>
        /// <exception cref="ArgumentOutOfRangeException">Изображение отсутствует</exception>
        public void UpdateImage(Guid imageId, string path, string alt) {
            ProductImage? productImage = images.FirstOrDefault(i => i.Id == imageId);
            if (productImage == null) { throw new ArgumentOutOfRangeException(nameof(imageId), "Изображение отсутствует"); }

            productImage.SetPath(path);
            productImage.SetAlt(alt);
        }

        /// <summary>
        /// Удалить изображение
        /// </summary>
        /// <param name="productImageId">Идентификатор изображения</param>
        public void RemoveImage(Guid productImageId) {
            ProductImage? image = images.FirstOrDefault(i => i.Id == productImageId);
            if (image != null) {
                images.Remove(image);
            }
        }

        /// <summary>
        /// Создает экземпляр объекта Product
        /// </summary>
        /// <param name="name">Название товара</param>
        /// <param name="isSale">Если true товар продается, false товар снят с продажи</param>
        /// <param name="price">Цена товара</param>
        /// <param name="sizes">Доступные размеры товара</param>
        /// <param name="dateAdded">Дата добавления</param>
        /// <param name="description">Краткое описание товара</param>
        /// <param name="content">Описание товара</param>
        /// <returns>Созданный объект</returns>
        public static Product Create(string name, bool isSale, double price, ProductSize sizes, DateTime dateAdded, string description, string content) {
            return new Product(Guid.Empty, name, isSale, price, sizes, dateAdded, description, content);
        }
    }
}
