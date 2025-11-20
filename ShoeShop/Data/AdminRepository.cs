using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Data {
    public class AdminRepository : IAdminRepository {

        public AdminRepository(ApplicationContext context) {
            this.context = context;
        }

        private readonly ApplicationContext context;

        public async Task<Product?> GetProduct(Guid productId) {
            return await context.Products.Include(p => p.Images).Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<IReadOnlyList<Product>> GetProducts(ProductSorting sorting, IsSaleFilter filter, string partProductName, int start, int count) {
            IQueryable<Product> query = context.Products.IsSaleFilters(filter).SearchByName(partProductName).OrderProductsBy(sorting).Page(start, count).Include(p => p.Images).Include(p => p.Category);
            string sql = query.ToQueryString();
            return await query.ToArrayAsync();
        }

        public async Task<int> ProductCount(IsSaleFilter filter, string partProductName) {
            return await context.Products.IsSaleFilters(filter).SearchByName(partProductName).CountAsync();
        }

        public async Task AddProduct(Product product) {
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product product) {
            context.Update(product);
            int a = await context.SaveChangesAsync();
        }

        public async Task RemoveProduct(Guid productId) {
            Product? product = await GetProduct(productId);
            if (product != null) {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<Order>> GetOrders(OrderStatusFilter filter, OrderSorting sorting, int start, int count) {
            IQueryable<Order> query = context.Orders
                .Include(o => o.Recipient)
                .Include(o => o.OrderDetails)
                .StatusFilter(filter)
                .OrderByDate(sorting)
                .Page(start, count);
            return await query.ToArrayAsync();
        }

        public async Task<int> OrderCount(OrderStatusFilter filter) {
            return await context.Orders.StatusFilter(filter).CountAsync();
        }
        
        public async Task<Dictionary<OrderStatus, int>> GetOrderStatsByStatus(OrderStatusFilter filter) {
            var query = context.Orders.StatusFilter(filter);
            
            var stats = await query
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
                
            // Заполняем отсутствующие статусы нулями
            var allStatuses = Enum.GetValues<OrderStatus>();
            foreach (var status in allStatuses) {
                if (!stats.ContainsKey(status)) {
                    stats[status] = 0;
                }
            }
            
            return stats;
        }

        public async Task<Order?> GetOrder(string orderId) {
            return await context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task UpdateOrder(Order order) {
            context.Update(order);
            await context.SaveChangesAsync();
        }

        public async Task DeleteOrder(string orderId) {
            Order? order = await GetOrder(orderId);
            if (order != null) {
                context.Orders.Remove(order);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllOrders() {
            // Сначала удаляем все OrderDetails
            context.OrderDetails.RemoveRange(context.OrderDetails);
            // Затем удаляем все Orders
            context.Orders.RemoveRange(context.Orders);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetCategories() {
            return await context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task AddCategory(Category category) {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }

        public async Task RemoveCategory(Guid categoryId) {
            Category? category = await context.Categories.FindAsync(categoryId);
            if (category != null) {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
            }
        }
        
        // Быстрые методы с минимальными запросами
        public async Task<IEnumerable<Order>> GetOrdersFast(OrderStatusFilter filter, OrderSorting sorting, int skip, int take) {
            return await context.Orders
                .Include(o => o.Recipient)
                .StatusFilter(filter)
                .OrderByDate(sorting)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }
        
        public async Task<int> OrderCountFast(OrderStatusFilter filter) {
            return await context.Orders
                .StatusFilter(filter)
                .AsNoTracking()
                .CountAsync();
        }
        
        private static Dictionary<OrderStatus, int>? _cachedStats;
        private static DateTime _cacheTime = DateTime.MinValue;
        
        public async Task<Dictionary<OrderStatus, int>> GetOrderStatsCache() {
            // Кеш на 60 секунд
            if (_cachedStats == null || DateTime.Now - _cacheTime > TimeSpan.FromSeconds(60)) {
                _cachedStats = await context.Orders
                    .GroupBy(o => o.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);
                    
                foreach (var status in Enum.GetValues<OrderStatus>()) {
                    if (!_cachedStats.ContainsKey(status)) {
                        _cachedStats[status] = 0;
                    }
                }
                _cacheTime = DateTime.Now;
            }
            return _cachedStats;
        }
        
        public async Task<Order?> GetOrderByNumber(string orderNumber) {
            return await context.Orders
                .Include(o => o.Recipient)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }
    }
}
