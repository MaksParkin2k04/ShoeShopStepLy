using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Attributes;
using ShoeShop.Models;
using ShoeShop.Services;
using ShoeShop.Infrastructure;

namespace ShoeShop.Pages.Admin
{
    [Authorize]
    [AdminAuth("Admin", "Manager", "Editor")]
    public class ProductsManagementModel : PageModel
    {
        private const int MAX_COUNT_ITEMS_ON_PAGE = 10;
        private const int STOCK_ITEMS_PER_PAGE = 20;

        private readonly IAdminRepository _adminRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductStockRepository _stockRepository;
        private readonly StockService _stockService;
        private readonly IProductManager _productManager;

        public ProductsManagementModel(
            IAdminRepository adminRepository,
            IProductRepository productRepository,
            IProductStockRepository stockRepository,
            StockService stockService,
            IProductManager productManager)
        {
            _adminRepository = adminRepository;
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _stockService = stockService;
            _productManager = productManager;
        }

        // Общие свойства
        public string? Message { get; set; }
        public string ActiveTab { get; set; } = "catalog";
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Product> AllProducts { get; set; } = new List<Product>();

        // Свойства для вкладки "Каталог"
        public int CurrentPage { get; private set; } = 1;
        public int ElementsPerPage { get; private set; } = MAX_COUNT_ITEMS_ON_PAGE;
        public int TotalElementsCount { get; private set; }
        public ProductSorting Sorting { get; private set; } = ProductSorting.Default;
        public IsSaleFilter IsSaleFilter { get; private set; } = Models.IsSaleFilter.All;
        public string PartProductName { get; private set; } = string.Empty;
        public IReadOnlyList<Product>? Products { get; private set; }

        // Свойства для вкладки "Остатки"
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
        public IEnumerable<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();

        // Свойства для вкладки "Категории"
        public IEnumerable<Category>? CategoriesList { get; private set; }

        public async Task OnGetAsync(
            string activeTab = "catalog",
            ProductSorting sorting = ProductSorting.Default,
            IsSaleFilter saleFilter = Models.IsSaleFilter.All,
            string partProductName = "",
            string? search = null,
            string? status = null,
            int pageIndex = 1)
        {
            ActiveTab = activeTab;
            Sorting = sorting;
            IsSaleFilter = saleFilter;
            PartProductName = partProductName;
            SearchQuery = search;
            StatusFilter = status;
            CurrentPage = pageIndex;

            // Загружаем общие данные
            Categories = await _adminRepository.GetCategories();
            AllProducts = await _productRepository.GetProducts(ProductSorting.Default, 0, 1000);
            
            // Всегда загружаем данные для категорий
            CategoriesList = Categories;

            // Загружаем данные в зависимости от активной вкладки
            if (ActiveTab == "catalog")
            {
                await LoadCatalogData();
            }
            else if (ActiveTab == "stock")
            {
                await LoadStockData();
            }
            // Для вкладки "categories" данные уже загружены в CategoriesList = Categories
        }

        private async Task LoadCatalogData()
        {
            ElementsPerPage = MAX_COUNT_ITEMS_ON_PAGE;
            TotalElementsCount = await _adminRepository.ProductCount(IsSaleFilter, PartProductName);
            Products = await _adminRepository.GetProducts(Sorting, IsSaleFilter, PartProductName, CurrentPage - 1, MAX_COUNT_ITEMS_ON_PAGE);
        }

        private async Task LoadStockData()
        {
            ElementsPerPage = STOCK_ITEMS_PER_PAGE;
            
            if (!string.IsNullOrEmpty(SearchQuery) || !string.IsNullOrEmpty(StatusFilter))
            {
                ProductStocks = await _stockRepository.GetStocksWithSearchAsync(SearchQuery, StatusFilter, (CurrentPage - 1) * ElementsPerPage, ElementsPerPage);
                TotalElementsCount = await _stockRepository.GetStocksCountWithSearchAsync(SearchQuery, StatusFilter);
            }
            else
            {
                ProductStocks = await _stockRepository.GetSimpleStocksAsync((CurrentPage - 1) * ElementsPerPage, ElementsPerPage);
                TotalElementsCount = await _stockRepository.GetTotalStocksCountAsync();
            }
        }

        public async Task<IActionResult> OnPostAddStockAsync(Guid productId, int size, int quantity, double purchasePrice = 0)
        {
            try
            {
                await _stockService.AddStockAsync(productId, size, quantity, purchasePrice);
                Message = $"Успешно добавлен приход: {quantity} пар размера {size}";
            }
            catch (Exception ex)
            {
                Message = $"Ошибка при добавлении прихода: {ex.Message}";
            }

            return RedirectToPage(new { activeTab = "stock" });
        }

        public async Task<IActionResult> OnPostReduceStockAsync(Guid productId, int size, int quantity)
        {
            try
            {
                await _stockService.ReduceStockAsync(productId, size, quantity);
                Message = $"Успешно списано: {quantity} пар размера {size}";
            }
            catch (Exception ex)
            {
                Message = $"Ошибка при списании: {ex.Message}";
            }

            return RedirectToPage(new { activeTab = "stock" });
        }

        public async Task<IActionResult> OnPostUpdatePriceAsync(Guid productId, int size, double purchasePrice)
        {
            try
            {
                await _stockService.UpdatePurchasePriceAsync(productId, size, purchasePrice);
                Message = $"Цена закупки обновлена: {purchasePrice:F2} ₽";
            }
            catch (Exception ex)
            {
                Message = $"Ошибка при обновлении цены: {ex.Message}";
            }

            return RedirectToPage(new { activeTab = "stock" });
        }

        public async Task<IActionResult> OnPostCreateProductAsync(
            string name,
            bool? isSale,
            double price,
            string description,
            string content,
            Guid categoryId,
            ulong[]? sizes)
        {
            try
            {
                // Валидация входных данных
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Название товара не может быть пустым");
                
                if (price <= 0)
                    throw new ArgumentException("Цена должна быть больше 0");
                
                if (string.IsNullOrWhiteSpace(description))
                    throw new ArgumentException("Краткое описание не может быть пустым");
                
                if (string.IsNullOrWhiteSpace(content))
                    throw new ArgumentException("Подробное описание не может быть пустым");
                
                if (categoryId == Guid.Empty)
                    throw new ArgumentException("Необходимо выбрать категорию");

                var editProduct = new EditProduct
                {
                    Name = name,
                    IsSale = isSale ?? true, // По умолчанию true, если чекбокс не отмечен
                    Price = price,
                    Description = description,
                    Content = content,
                    CategoryId = categoryId,
                    Sizes = ProductSize.Not
                };

                if (sizes != null && sizes.Length > 0)
                {
                    ProductSize combinedSizes = ProductSize.Not;
                    foreach (ulong size in sizes)
                    {
                        combinedSizes |= (ProductSize)size;
                    }
                    editProduct.Sizes = combinedSizes;
                }

                // Валидация модели
                editProduct.Validate();

                // Отладочная информация
                Console.WriteLine($"Создание товара: Name={editProduct.Name}, IsSale={editProduct.IsSale}, Price={editProduct.Price}, CategoryId={editProduct.CategoryId}");
                Console.WriteLine($"Размеры: {editProduct.Sizes}");

                Guid productId = await _productManager.Add(editProduct);
                Message = $"Товар '{name}' успешно создан! ID: {productId}";
                
                // Переходим на вкладку каталога
                return RedirectToPage(new { activeTab = "catalog" });
            }
            catch (Exception ex)
            {
                Message = $"Ошибка при создании товара: {ex.Message}";
                Console.WriteLine($"Ошибка создания товара: {ex}");
                return RedirectToPage(new { activeTab = "add" });
            }
        }



        public async Task<IActionResult> OnPostAddCategoryAsync(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                Category category = Category.Create(categoryName.Trim());
                await _adminRepository.AddCategory(category);
                Message = $"Категория '{categoryName}' успешно добавлена!";
            }
            return RedirectToPage(new { activeTab = "categories" });
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(Guid categoryId)
        {
            await _adminRepository.RemoveCategory(categoryId);
            Message = "Категория успешно удалена!";
            return RedirectToPage(new { activeTab = "categories" });
        }
    }
}