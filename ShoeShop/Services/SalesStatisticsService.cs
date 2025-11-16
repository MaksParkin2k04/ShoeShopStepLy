using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Services {
    /// <summary>
    /// Сервис для расчета статистики продаж
    /// </summary>
    public class SalesStatisticsService {
        private readonly ApplicationContext _context;
        private readonly IProductStockRepository _stockRepository;

        public SalesStatisticsService(ApplicationContext context, IProductStockRepository stockRepository) {
            _context = context;
            _stockRepository = stockRepository;
        }

        /// <summary>
        /// Получить общую статистику продаж
        /// </summary>
        public async Task<SalesStatistics> GetSalesStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.Status == OrderStatus.Completed);

            if (fromDate.HasValue) {
                query = query.Where(o => o.CreatedDate >= fromDate.Value);
            }
            if (toDate.HasValue) {
                query = query.Where(o => o.CreatedDate <= toDate.Value);
            }

            var orders = await query.ToListAsync();
            var statistics = new SalesStatistics();
            
            foreach (var order in orders) {
                foreach (var detail in order.OrderDetails) {
                    statistics.TotalQuantitySold++;
                    statistics.TotalRevenue += detail.Price;
                }
            }

            // Затраты считаем по всем поступлениям на склад
            statistics.TotalCosts = await _context.ProductStocks
                .Where(ps => ps.PurchasePrice > 0)
                .SumAsync(ps => ps.Quantity * ps.PurchasePrice);

            return statistics;
        }

        /// <summary>
        /// Получить статистику по товарам
        /// </summary>
        public async Task<List<ProductSalesStatistics>> GetProductSalesStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.Status == OrderStatus.Completed);

            if (fromDate.HasValue) {
                query = query.Where(o => o.CreatedDate >= fromDate.Value);
            }
            if (toDate.HasValue) {
                query = query.Where(o => o.CreatedDate <= toDate.Value);
            }

            var orders = await query.ToListAsync();
            var productStats = new Dictionary<Guid, ProductSalesStatistics>();

            foreach (var order in orders) {
                foreach (var detail in order.OrderDetails) {
                    if (!productStats.ContainsKey(detail.ProductId)) {
                        productStats[detail.ProductId] = new ProductSalesStatistics {
                            ProductName = detail.Name,
                            ProductId = detail.ProductId
                        };
                    }

                    var stats = productStats[detail.ProductId];
                    stats.QuantitySold++;
                    stats.Revenue += detail.Price;
                }
            }

            // Затраты по товарам считаем по всем поступлениям на склад
            foreach (var stats in productStats.Values) {
                stats.Costs = await _context.ProductStocks
                    .Where(ps => ps.ProductId == stats.ProductId && ps.PurchasePrice > 0)
                    .SumAsync(ps => ps.Quantity * ps.PurchasePrice);
            }

            return productStats.Values.OrderByDescending(p => p.Revenue).ToList();
        }
    }
}