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
        public string ActiveTab { get; set; } = "search";

        public async Task OnGetAsync(string? search, string? tab)
        {
            SearchTerm = search ?? "";
            ActiveTab = tab ?? "search";
            
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Products = (await repository.GetAllAsync())
                    .Where(p => p.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .Take(20)
                    .ToList();
                
                // Загружаем остатки для найденных товаров
                await LoadProductStocks(Products);
            }

            // Загружаем все товары для вкладки "Все товары"
            if (ActiveTab == "products")
            {
                AllProducts = (await repository.GetAllAsync()).Take(50).ToList();
                // Загружаем остатки для всех товаров
                await LoadProductStocks(AllProducts);
            }

            // Загружаем последние заказы для вкладки "Заказы"
            if (ActiveTab == "orders")
            {
                RecentOrders = (await adminRepository.GetOrders(OrderStatusFilter.All, OrderSorting.ByDateDesc, 0, 20)).ToList();
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