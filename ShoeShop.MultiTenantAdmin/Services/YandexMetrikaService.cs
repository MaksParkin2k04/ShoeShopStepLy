using System.Text.Json;

namespace ShoeShop.MultiTenantAdmin.Services
{
    public class YandexMetrikaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _counterId;
        private readonly string _token;
        private readonly bool _useTestData;

        public YandexMetrikaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _counterId = configuration["YandexMetrika:CounterId"] ?? "98765432";
            _token = configuration["YandexMetrika:OAuthToken"] ?? "test_token";
            _useTestData = configuration.GetValue<bool>("YandexMetrika:UseTestData", true);
        }

        public async Task<MetrikaStats> GetStatsAsync()
        {
            // Для localhost или тестирования используем тестовые данные
            if (_useTestData || string.IsNullOrEmpty(_token) || _token == "test_token")
            {
                return GetTestData();
            }
            
            try
            {
                var endDate = DateTime.Now.ToString("yyyy-MM-dd");
                var startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                
                var url = $"https://api-metrika.yandex.net/stat/v1/data?id={_counterId}&date1={startDate}&date2={endDate}&metrics=ym:s:visits,ym:s:pageviews,ym:s:users&oauth_token={_token}";
                
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<MetrikaResponse>(json);
                    
                    return new MetrikaStats
                    {
                        Visits = data?.totals?[0] ?? 0,
                        PageViews = data?.totals?[1] ?? 0,
                        Users = data?.totals?[2] ?? 0,
                        Period = $"{startDate} - {endDate} (Реальные данные)"
                    };
                }
            }
            catch
            {
                // В случае ошибки возвращаем тестовые данные
            }
            
            return GetTestData();
        }
        
        private MetrikaStats GetTestData()
        {
            
            return new MetrikaStats
            {
                Visits = 1250,
                PageViews = 3420,
                Users = 890,
                BounceRate = 45.2,
                AvgVisitDuration = 185.5,
                NewUsers = 623,
                ReturningUsers = 267,
                MobileUsers = 534,
                DesktopUsers = 356,
                TabletUsers = 89,
                ConversionRate = 3.2,
                Goals = 28,
                TopPages = new List<TopPage>
                {
                    new() { Url = "/", Views = 856 },
                    new() { Url = "/Catalog", Views = 432 },
                    new() { Url = "/Product", Views = 298 },
                    new() { Url = "/About", Views = 156 },
                    new() { Url = "/Delivery", Views = 89 }
                },
                TrafficSources = new List<TrafficSource>
                {
                    new() { Source = "Прямые заходы", Users = 356, Percentage = 40.0 },
                    new() { Source = "Поисковые системы", Users = 267, Percentage = 30.0 },
                    new() { Source = "Социальные сети", Users = 178, Percentage = 20.0 },
                    new() { Source = "Реферальные ссылки", Users = 89, Percentage = 10.0 }
                },
                TopCountries = new List<CountryStats>
                {
                    new() { Country = "Россия", Users = 712, Percentage = 80.0 },
                    new() { Country = "Казахстан", Users = 89, Percentage = 10.0 },
                    new() { Country = "Беларусь", Users = 53, Percentage = 6.0 },
                    new() { Country = "Украина", Users = 36, Percentage = 4.0 }
                },
                TopBrowsers = new List<BrowserStats>
                {
                    new() { Browser = "Chrome", Users = 445, Percentage = 50.0 },
                    new() { Browser = "Safari", Users = 178, Percentage = 20.0 },
                    new() { Browser = "Firefox", Users = 133, Percentage = 15.0 },
                    new() { Browser = "Edge", Users = 89, Percentage = 10.0 },
                    new() { Browser = "Прочие", Users = 45, Percentage = 5.0 }
                },
                HourlyData = new List<HourlyStats>
                {
                    new() { Hour = 9, Visits = 45 },
                    new() { Hour = 10, Visits = 67 },
                    new() { Hour = 11, Visits = 89 },
                    new() { Hour = 12, Visits = 123 },
                    new() { Hour = 13, Visits = 98 },
                    new() { Hour = 14, Visits = 156 },
                    new() { Hour = 15, Visits = 134 },
                    new() { Hour = 16, Visits = 178 },
                    new() { Hour = 17, Visits = 145 },
                    new() { Hour = 18, Visits = 167 },
                    new() { Hour = 19, Visits = 134 },
                    new() { Hour = 20, Visits = 114 }
                },
                TopSearchQueries = new List<SearchQuery>
                {
                    new() { Query = "кроссовки мужские", Count = 156 },
                    new() { Query = "ботинки женские", Count = 134 },
                    new() { Query = "обувь детская", Count = 89 },
                    new() { Query = "зимняя обувь", Count = 67 },
                    new() { Query = "спортивная обувь", Count = 45 }
                },
                TopExitPages = new List<ExitPage>
                {
                    new() { Url = "/Product", Exits = 234, ExitRate = 45.2 },
                    new() { Url = "/Catalog", Exits = 156, ExitRate = 36.1 },
                    new() { Url = "/BasketShopping", Exits = 89, ExitRate = 28.3 },
                    new() { Url = "/About", Exits = 67, ExitRate = 42.9 },
                    new() { Url = "/Delivery", Exits = 45, ExitRate = 50.6 }
                },
                AvgPageDepth = 2.8,
                TotalSessions = 1450,
                PageLoadTime = 1.2,
                ErrorPages = 12,
                Period = "Тестовые данные за 30 дней"
            };
        }
    }

    public class MetrikaStats
    {
        public int Visits { get; set; }
        public int PageViews { get; set; }
        public int Users { get; set; }
        public double BounceRate { get; set; }
        public double AvgVisitDuration { get; set; }
        public int NewUsers { get; set; }
        public int MobileUsers { get; set; }
        public int DesktopUsers { get; set; }
        public int TabletUsers { get; set; }
        public int ReturningUsers { get; set; }
        public double ConversionRate { get; set; }
        public int Goals { get; set; }
        public List<TopPage> TopPages { get; set; } = new();
        public List<TrafficSource> TrafficSources { get; set; } = new();
        public List<CountryStats> TopCountries { get; set; } = new();
        public List<BrowserStats> TopBrowsers { get; set; } = new();
        public List<HourlyStats> HourlyData { get; set; } = new();
        public List<SearchQuery> TopSearchQueries { get; set; } = new();
        public List<ExitPage> TopExitPages { get; set; } = new();
        public double AvgPageDepth { get; set; }
        public int TotalSessions { get; set; }
        public double PageLoadTime { get; set; }
        public int ErrorPages { get; set; }
        public string Period { get; set; } = "";
    }

    public class TopPage
    {
        public string Url { get; set; } = "";
        public int Views { get; set; }
    }

    public class TrafficSource
    {
        public string Source { get; set; } = "";
        public int Users { get; set; }
        public double Percentage { get; set; }
    }

    public class CountryStats
    {
        public string Country { get; set; } = "";
        public int Users { get; set; }
        public double Percentage { get; set; }
    }

    public class BrowserStats
    {
        public string Browser { get; set; } = "";
        public int Users { get; set; }
        public double Percentage { get; set; }
    }

    public class HourlyStats
    {
        public int Hour { get; set; }
        public int Visits { get; set; }
    }

    public class SearchQuery
    {
        public string Query { get; set; } = "";
        public int Count { get; set; }
    }

    public class ExitPage
    {
        public string Url { get; set; } = "";
        public int Exits { get; set; }
        public double ExitRate { get; set; }
    }

    public class MetrikaResponse
    {
        public int[]? totals { get; set; }
    }
}
