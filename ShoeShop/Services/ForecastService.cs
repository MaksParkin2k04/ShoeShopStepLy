using ShoeShop.Data;
using Microsoft.EntityFrameworkCore;

namespace ShoeShop.Services {
    public class ForecastService {
        private readonly ApplicationContext _context;

        public ForecastService(ApplicationContext context) {
            _context = context;
        }

        public async Task<ForecastData> GetSalesForecastAsync() {
            var last30Days = await GetDailySalesLast30DaysAsync();
            if (last30Days.Count < 7) return new ForecastData();

            var avgDailyRevenue = last30Days.Average(d => d.Revenue);
            var avgDailyOrders = last30Days.Average(d => d.OrderCount);
            var trend = CalculateTrend(last30Days);

            return new ForecastData {
                NextWeekRevenue = avgDailyRevenue * 7 * (decimal)(1 + trend),
                NextMonthRevenue = avgDailyRevenue * 30 * (decimal)(1 + trend),
                NextWeekOrders = (int)(avgDailyOrders * 7 * (1 + trend)),
                TrendPercentage = (decimal)(trend * 100),
                Recommendations = GenerateRecommendations(trend, avgDailyRevenue, last30Days)
            };
        }

        private async Task<List<DailySalesData>> GetDailySalesLast30DaysAsync() {
            var startDate = DateTime.Now.AddDays(-30);
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.CreatedDate >= startDate && o.Status == Models.OrderStatus.Completed)
                .ToListAsync();

            return orders.GroupBy(o => o.CreatedDate.Date)
                .Select(g => new DailySalesData {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.OrderDetails.Sum(d => (decimal)d.Price)),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();
        }

        private double CalculateTrend(List<DailySalesData> data) {
            if (data.Count < 2) return 0;
            
            var firstHalf = data.Take(data.Count / 2).Average(d => d.Revenue);
            var secondHalf = data.Skip(data.Count / 2).Average(d => d.Revenue);
            
            return firstHalf > 0 ? (double)((secondHalf - firstHalf) / firstHalf) : 0;
        }

        private List<string> GenerateRecommendations(double trend, decimal avgRevenue, List<DailySalesData> data) {
            var recommendations = new List<string>();

            if (trend < -0.1) {
                recommendations.Add("📉 Продажи снижаются. Рассмотрите акции или новые товары");
            } else if (trend > 0.1) {
                recommendations.Add("📈 Отличный рост! Увеличьте закупки популярных товаров");
            }

            var weekdays = data.Where(d => d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday);
            var weekends = data.Where(d => d.Date.DayOfWeek == DayOfWeek.Saturday || d.Date.DayOfWeek == DayOfWeek.Sunday);
            
            if (weekends.Any() && weekdays.Any() && weekends.Average(d => d.Revenue) > weekdays.Average(d => d.Revenue) * 1.2m) {
                recommendations.Add("🎯 Выходные дни более прибыльны - усильте маркетинг на выходные");
            }

            if (avgRevenue < 10000) {
                recommendations.Add("💡 Средняя выручка низкая - рассмотрите повышение среднего чека");
            }

            return recommendations;
        }

        public async Task<List<AlertData>> GetAlertsAsync() {
            var alerts = new List<AlertData>();
            var yesterday = DateTime.Now.AddDays(-1).Date;
            var weekAgo = DateTime.Now.AddDays(-7).Date;

            // Проверка резкого падения продаж
            var yesterdayRevenue = await GetDayRevenueAsync(yesterday);
            var avgWeekRevenue = await GetAverageRevenueAsync(weekAgo, yesterday.AddDays(-1));
            
            if (avgWeekRevenue > 0 && yesterdayRevenue < avgWeekRevenue * 0.5m) {
                alerts.Add(new AlertData {
                    Type = "warning",
                    Message = $"⚠️ Резкое падение продаж: {yesterdayRevenue:F0}₽ против среднего {avgWeekRevenue:F0}₽",
                    Date = DateTime.Now
                });
            }

            // Проверка остатков
            var lowStock = await _context.ProductStocks
                .Include(ps => ps.Product)
                .Where(ps => ps.Quantity <= 2)
                .ToListAsync();

            if (lowStock.Any()) {
                alerts.Add(new AlertData {
                    Type = "info",
                    Message = $"📦 Заканчиваются товары: {lowStock.Count} позиций",
                    Date = DateTime.Now
                });
            }

            return alerts;
        }

        private async Task<decimal> GetDayRevenueAsync(DateTime date) {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.CreatedDate.Date == date && o.Status == Models.OrderStatus.Completed)
                .SelectMany(o => o.OrderDetails)
                .SumAsync(d => (decimal)d.Price);
        }

        private async Task<decimal> GetAverageRevenueAsync(DateTime from, DateTime to) {
            var days = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.CreatedDate.Date >= from && o.CreatedDate.Date <= to && o.Status == Models.OrderStatus.Completed)
                .GroupBy(o => o.CreatedDate.Date)
                .Select(g => g.SelectMany(o => o.OrderDetails).Sum(d => (decimal)d.Price))
                .ToListAsync();

            return days.Any() ? days.Average() : 0;
        }
    }

    public class ForecastData {
        public decimal NextWeekRevenue { get; set; }
        public decimal NextMonthRevenue { get; set; }
        public int NextWeekOrders { get; set; }
        public decimal TrendPercentage { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    public class AlertData {
        public string Type { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime Date { get; set; }
    }

    public class DailySalesData {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}