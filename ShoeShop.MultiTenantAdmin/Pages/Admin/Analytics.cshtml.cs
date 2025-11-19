using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.Services;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AnalyticsModel : PageModel
    {
        private readonly YandexMetrikaService _metrikaService;

        public AnalyticsModel(YandexMetrikaService metrikaService)
        {
            _metrikaService = metrikaService;
        }

        public MetrikaStats MetrikaStats { get; set; } = new();

        public async Task OnGetAsync()
        {
            MetrikaStats = await _metrikaService.GetStatsAsync();
        }
    }
}
