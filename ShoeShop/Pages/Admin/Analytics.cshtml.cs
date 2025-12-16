using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Services;

namespace ShoeShop.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AnalyticsModel : PageModel
    {
        private readonly YandexMetrikaService _metrikaService;
        private readonly ILogger<AnalyticsModel> _logger;

        public AnalyticsModel(YandexMetrikaService metrikaService, ILogger<AnalyticsModel> logger)
        {
            _metrikaService = metrikaService;
            _logger = logger;
        }

        public MetrikaStats MetrikaStats { get; set; } = new();

        public async Task OnGetAsync()
        {
            _logger.LogInformation("Analytics page OnGetAsync called");
            try
            {
                MetrikaStats = await _metrikaService.GetStatsAsync();
                _logger.LogInformation($"MetrikaStats received: Period = {MetrikaStats.Period}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting Metrika stats: {ex.Message}");
                MetrikaStats = new MetrikaStats();
            }
        }
    }
}