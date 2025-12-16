using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;
using ShoeShop.Services;
using static ShoeShop.Services.ForecastService;

namespace ShoeShop.Pages.Admin {
    [Authorize(Roles = "Admin,Manager,Analyst")]
    public class StatisticsModel : PageModel {
        private readonly SalesStatisticsService _statisticsService;
        private readonly ExportService _exportService;
        private readonly ForecastService _forecastService;

        public StatisticsModel(SalesStatisticsService statisticsService, ExportService exportService, ForecastService forecastService) {
            _statisticsService = statisticsService;
            _exportService = exportService;
            _forecastService = forecastService;
        }

        public SalesStatistics Statistics { get; set; } = new SalesStatistics();
        public List<ProductSalesStatistics> ProductStatistics { get; set; } = new List<ProductSalesStatistics>();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Dictionary<int, int> SizeStatistics { get; set; } = new();
        public Dictionary<string, decimal> CategoryStatistics { get; set; } = new();
        public List<DailySales> DailySalesData { get; set; } = new();
        public Dictionary<string, int> OrderStatusStats { get; set; } = new();
        public decimal ConversionRate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<TopProduct> TopProducts { get; set; } = new();
        public ForecastData Forecast { get; set; } = new();
        public List<AlertData> Alerts { get; set; } = new();

        public async Task OnGetAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            FromDate = fromDate;
            ToDate = toDate;

            Statistics = await _statisticsService.GetSalesStatisticsAsync(fromDate, toDate);
            ProductStatistics = await _statisticsService.GetProductSalesStatisticsAsync(fromDate, toDate);
            SizeStatistics = await _statisticsService.GetSizeStatisticsAsync(fromDate, toDate);
            CategoryStatistics = await _statisticsService.GetCategoryStatisticsAsync(fromDate, toDate);
            DailySalesData = await _statisticsService.GetDailySalesAsync(fromDate, toDate);
            OrderStatusStats = await _statisticsService.GetOrderStatusStatisticsAsync(fromDate, toDate);
            ConversionRate = await _statisticsService.GetConversionRateAsync(fromDate, toDate);
            AverageOrderValue = await _statisticsService.GetAverageOrderValueAsync(fromDate, toDate);
            TopProducts = await _statisticsService.GetTopProductsAsync(fromDate, toDate);
            Forecast = await _forecastService.GetSalesForecastAsync();
            Alerts = await _forecastService.GetAlertsAsync();
        }
        
        public async Task<IActionResult> OnGetExportAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            var excelData = await _exportService.ExportStatisticsToExcel();
            var fileName = $"STEPLY_Отчет_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
            
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}