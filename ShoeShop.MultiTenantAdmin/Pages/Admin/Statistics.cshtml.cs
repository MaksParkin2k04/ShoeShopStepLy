using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.Services;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin {
    [Authorize(Roles = "Admin")]
    public class StatisticsModel : PageModel {
        private readonly SalesStatisticsService _statisticsService;

        public StatisticsModel(SalesStatisticsService statisticsService) {
            _statisticsService = statisticsService;
        }

        public SalesStatistics Statistics { get; set; } = new SalesStatistics();
        public List<ProductSalesStatistics> ProductStatistics { get; set; } = new List<ProductSalesStatistics>();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public async Task OnGetAsync(DateTime? fromDate = null, DateTime? toDate = null) {
            FromDate = fromDate;
            ToDate = toDate;

            Statistics = await _statisticsService.GetSalesStatisticsAsync(fromDate, toDate);
            ProductStatistics = await _statisticsService.GetProductSalesStatisticsAsync(fromDate, toDate);
        }
    }
}
