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
                .Where(o => o.Status >= OrderStatus.Paid);

            if (fromDate.HasValue) {
                query = query.Where(o => o.CreatedDate >= fromDate.Value);
            }
            if (toDate.HasValue) {
                query = query.Where(o => o.CreatedDate <= toDate.Value);
            }

            var orders = await query.ToListAsync();
            var statistics = new SalesStatistics();
            
            // Рассчитываем затраты: остатки + проданные товары
            var allStocks = await _context.ProductStocks
                .Where(ps => ps.PurchasePrice > 0)
                .ToListAsync();
            
            // Затраты на остатки
            foreach (var stock in allStocks) {
                statistics.TotalCosts += stock.PurchasePrice * stock.Quantity;
            }
            
            // Добавляем затраты на проданные товары
            foreach (var order in orders) {
                foreach (var detail in order.OrderDetails) {
                    var stock = await _context.ProductStocks
                        .Where(ps => ps.ProductId == detail.ProductId && ps.Size == detail.Size)
                        .FirstOrDefaultAsync();
                    
                    if (stock != null && stock.PurchasePrice > 0) {
                        statistics.TotalCosts += stock.PurchasePrice;
                    }
                }
            }
            
            foreach (var order in orders) {
                foreach (var detail in order.OrderDetails) {
                    statistics.TotalQuantitySold++;
                    statistics.TotalRevenue += detail.Price;
                }
            }

            return statistics;
        }

        /// <summary>
        /// Получить статистику по товарам
        /// </summary>
        public async Task<List<ProductSalesStatistics>> GetProductSalesStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.Status >= OrderStatus.Paid);

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

            // Добавляем затраты на остатки для всех товаров
            var allStocks = await _context.ProductStocks
                .Include(ps => ps.Product)
                .Where(ps => ps.PurchasePrice > 0)
                .ToListAsync();
            
            foreach (var stock in allStocks) {
                if (!productStats.ContainsKey(stock.ProductId)) {
                    productStats[stock.ProductId] = new ProductSalesStatistics {
                        ProductId = stock.ProductId,
                        ProductName = stock.Product?.Name ?? "Неизвестный товар",
                        QuantitySold = 0,
                        Revenue = 0,
                        Costs = 0
                    };
                }
                
                // Затраты на остатки
                productStats[stock.ProductId].Costs += stock.PurchasePrice * stock.Quantity;
            }
            
            // Добавляем затраты на проданные товары
            foreach (var order in orders) {
                foreach (var detail in order.OrderDetails) {
                    if (productStats.ContainsKey(detail.ProductId)) {
                        var stock = await _context.ProductStocks
                            .Where(ps => ps.ProductId == detail.ProductId && ps.Size == detail.Size)
                            .FirstOrDefaultAsync();
                        
                        if (stock != null && stock.PurchasePrice > 0) {
                            productStats[detail.ProductId].Costs += stock.PurchasePrice;
                        }
                    }
                }
            }

            return productStats.Values.OrderByDescending(p => p.Revenue).ToList();
        }
        
        /// <summary>
        /// Статистика по размерам
        /// </summary>
        public async Task<Dictionary<int, int>> GetSizeStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.Status >= OrderStatus.Paid);

            if (fromDate.HasValue) {
                query = query.Where(o => o.CreatedDate >= fromDate.Value);
            }
            if (toDate.HasValue) {
                query = query.Where(o => o.CreatedDate <= toDate.Value);
            }

            var orders = await query.ToListAsync();
            var sizeStats = new Dictionary<int, int>();

            foreach (var order in orders) {
                foreach (var detail in order.OrderDetails) {
                    if (!sizeStats.ContainsKey(detail.Size)) {
                        sizeStats[detail.Size] = 0;
                    }
                    sizeStats[detail.Size]++;
                }
            }

            return sizeStats.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }
        
        /// <summary>
        /// Статистика по категориям
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetCategoryStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.Status >= OrderStatus.Paid);

            if (fromDate.HasValue) {
                query = query.Where(o => o.CreatedDate >= fromDate.Value);
            }
            if (toDate.HasValue) {
                query = query.Where(o => o.CreatedDate <= toDate.Value);
            }

            // Оптимизированный запрос с JOIN
            var categoryStats = await query
                .SelectMany(o => o.OrderDetails)
                .Join(_context.Products.Include(p => p.Category),
                    detail => detail.ProductId,
                    product => product.Id,
                    (detail, product) => new {
                        CategoryName = product.Category != null ? product.Category.Name : "Без категории",
                        Price = (decimal)detail.Price
                    })
                .GroupBy(x => x.CategoryName)
                .Select(g => new { Category = g.Key, Total = g.Sum(x => x.Price) })
                .OrderByDescending(x => x.Total)
                .ToDictionaryAsync(x => x.Category, x => x.Total);

            return categoryStats;
        }
        
        /// <summary>
        /// Продажи по дням
        /// </summary>
        public async Task<List<DailySales>> GetDailySalesAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.Status >= OrderStatus.Paid);

            if (fromDate.HasValue) {
                query = query.Where(o => o.CreatedDate >= fromDate.Value);
            }
            if (toDate.HasValue) {
                query = query.Where(o => o.CreatedDate <= toDate.Value);
            }

            var orders = await query.ToListAsync();
            var dailyStats = new Dictionary<DateTime, DailySales>();



            foreach (var order in orders) {
                var date = order.CreatedDate.Date;
                if (!dailyStats.ContainsKey(date)) {
                    dailyStats[date] = new DailySales { Date = date };
                }
                
                dailyStats[date].OrderCount++;
                dailyStats[date].Revenue += (decimal)order.OrderDetails.Sum(d => d.Price);
            }

            return dailyStats.Values.OrderBy(x => x.Date).ToList();
        }
        
        /// <summary>
        /// Статистика по статусам заказов
        /// </summary>
        public async Task<Dictionary<string, int>> GetOrderStatusStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            var query = _context.Orders.AsQueryable();

            if (fromDate.HasValue) {
                query = query.Where(o => o.CreatedDate >= fromDate.Value);
            }
            if (toDate.HasValue) {
                query = query.Where(o => o.CreatedDate <= toDate.Value);
            }

            var orders = await query.ToListAsync();
            var statusStats = new Dictionary<string, int>();

            foreach (var order in orders) {
                var statusName = order.Status switch {
                    OrderStatus.Created => "Создан",
                    OrderStatus.Paid => "Оплачен",
                    OrderStatus.Processing => "В обработке",
                    OrderStatus.AwaitingShipment => "Ожидает отправления",
                    OrderStatus.Shipped => "Отправлен",
                    OrderStatus.InTransit => "В пути",
                    OrderStatus.Arrived => "Прибыл",
                    OrderStatus.ReadyForPickup => "Готов к выдаче",
                    OrderStatus.Completed => "Выполнен",
                    OrderStatus.Returned => "Возвращен",
                    OrderStatus.Canceled => "Отменен",
                    _ => "Неизвестно"
                };
                
                if (!statusStats.ContainsKey(statusName)) {
                    statusStats[statusName] = 0;
                }
                statusStats[statusName]++;
            }

            return statusStats;
        }
        
        /// <summary>
        /// Конверсия заказов
        /// </summary>
        public async Task<decimal> GetConversionRateAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            var query = _context.Orders.AsQueryable();
            if (fromDate.HasValue) query = query.Where(o => o.CreatedDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(o => o.CreatedDate <= toDate.Value);
            
            var totalOrders = await query.CountAsync();
            var completedOrders = await query.CountAsync(o => o.Status >= OrderStatus.Paid);
            
            return totalOrders > 0 ? (decimal)completedOrders / totalOrders * 100 : 0;
        }
        
        /// <summary>
        /// Средний чек
        /// </summary>
        public async Task<decimal> GetAverageOrderValueAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            var query = _context.Orders.Include(o => o.OrderDetails).Where(o => o.Status >= OrderStatus.Paid);
            if (fromDate.HasValue) query = query.Where(o => o.CreatedDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(o => o.CreatedDate <= toDate.Value);
            
            var orders = await query.ToListAsync();
            if (!orders.Any()) return 0;
            
            var totalRevenue = orders.Sum(o => o.OrderDetails.Sum(d => (decimal)d.Price));
            return totalRevenue / orders.Count;
        }
        
        /// <summary>
        /// Топ товары
        /// </summary>
        public async Task<List<TopProduct>> GetTopProductsAsync(DateTime? fromDate = null, DateTime? toDate = null, int limit = 5) {
            var query = _context.Orders.Include(o => o.OrderDetails).Where(o => o.Status >= OrderStatus.Paid);
            if (fromDate.HasValue) query = query.Where(o => o.CreatedDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(o => o.CreatedDate <= toDate.Value);
            
            var orders = await query.ToListAsync();
            return orders.SelectMany(o => o.OrderDetails)
                .GroupBy(d => new { d.ProductId, d.Name })
                .Select(g => new TopProduct {
                    Name = g.Key.Name,
                    Quantity = g.Count(),
                    Revenue = g.Sum(d => (decimal)d.Price)
                })
                .OrderByDescending(p => p.Quantity)
                .Take(limit)
                .ToList();
        }
    }
    
    public class DailySales {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
    
    public class TopProduct {
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }
}