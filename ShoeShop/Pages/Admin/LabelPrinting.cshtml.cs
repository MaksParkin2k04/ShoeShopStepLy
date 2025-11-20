using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;
using ShoeShop.Data;

namespace ShoeShop.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class LabelPrintingModel : PageModel
    {
        private readonly IProductRepository repository;
        private readonly IProductStockRepository stockRepository;
        private readonly IAdminRepository adminRepository;

        public LabelPrintingModel(IProductRepository repository, IProductStockRepository stockRepository, IAdminRepository adminRepository)
        {
            this.repository = repository;
            this.stockRepository = stockRepository;
            this.adminRepository = adminRepository;
        }

        public List<Product> Products { get; set; } = new();
        public List<Product> AllProducts { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();
        public Dictionary<Guid, Dictionary<int, int>> ProductStocks { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string OrderSearch { get; set; } = "";
        public Order? FoundOrder { get; set; }
        public string ActiveTab { get; set; } = "search";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalOrdersCount { get; set; } = 0;
        public int TotalProductsCount { get; set; } = 0;

        public async Task OnGetAsync(string? search, string? orderSearch, string? tab, int page = 1)
        {
            SearchTerm = search ?? "";
            OrderSearch = orderSearch ?? "";
            ActiveTab = tab ?? "search";
            CurrentPage = page;
            
            // Поиск заказа по номеру
            if (!string.IsNullOrEmpty(OrderSearch) && ActiveTab == "order-search")
            {
                // Сначала поиск по короткому номеру
                FoundOrder = await adminRepository.GetOrderByNumber(OrderSearch);
                
                // Если не найден по короткому номеру, попробуем найти по самому значению как Id
                if (FoundOrder == null)
                {
                    FoundOrder = await adminRepository.GetOrder(OrderSearch);
                }
            }
            
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Products = (await repository.GetAllAsync())
                    .Where(p => p.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .Take(10)
                    .ToList();
                
                // Загружаем остатки для найденных товаров
                await LoadProductStocks(Products);
            }

            // Загружаем все товары для вкладки "Все товары" с пагинацией
            if (ActiveTab == "products")
            {
                var allProducts = await repository.GetAllAsync();
                TotalProductsCount = allProducts.Count();
                TotalPages = (int)Math.Ceiling((double)TotalProductsCount / PageSize);
                AllProducts = allProducts.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
                // Загружаем остатки для товаров на текущей странице
                await LoadProductStocks(AllProducts);
            }

            // Загружаем заказы для вкладки "Заказы" с пагинацией
            if (ActiveTab == "orders")
            {
                TotalOrdersCount = await adminRepository.OrderCount(OrderStatusFilter.All);
                TotalPages = (int)Math.Ceiling((double)TotalOrdersCount / PageSize);
                RecentOrders = (await adminRepository.GetOrders(OrderStatusFilter.All, OrderSorting.ByDateDesc, CurrentPage - 1, PageSize)).ToList();
            }
        }

        private async Task LoadProductStocks(List<Product> products)
        {
            foreach (var product in products)
            {
                var productStocks = new Dictionary<int, int>();
                var availableSizes = GetAvailableSizes(product.Sizes);
                
                foreach (var size in availableSizes)
                {
                    productStocks[size] = await stockRepository.GetQuantityAsync(product.Id, size);
                }
                
                ProductStocks[product.Id] = productStocks;
            }
        }
        
        public int GetStockQuantity(Guid productId, int size)
        {
            return ProductStocks.ContainsKey(productId) && ProductStocks[productId].ContainsKey(size) 
                ? ProductStocks[productId][size] 
                : 0;
        }

        public List<int> GetAvailableSizes(ProductSize sizes)
        {
            var sizeList = new List<int>();
            
            for (int i = 1; i <= 64; i++)
            {
                var sizeFlag = (ProductSize)(1UL << (i - 1));
                if (sizes.HasFlag(sizeFlag))
                {
                    sizeList.Add(i);
                }
            }
            
            return sizeList;
        }
    }
}