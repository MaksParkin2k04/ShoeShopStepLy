using Microsoft.AspNetCore.Identity.UI.Services;

namespace ShoeShop.Services {
    public class WeeklyReportService : BackgroundService {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WeeklyReportService> _logger;

        public WeeklyReportService(IServiceProvider serviceProvider, ILogger<WeeklyReportService> logger) {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                try {
                    var now = DateTime.Now;
                    // Отправляем отчет каждый понедельник в 9:00
                    if (now.DayOfWeek == DayOfWeek.Monday && now.Hour == 9 && now.Minute < 5) {
                        await SendWeeklyReportAsync();
                    }
                } catch (Exception ex) {
                    _logger.LogError(ex, "Ошибка при отправке еженедельного отчета");
                }

                // Проверяем каждые 5 минут
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task SendWeeklyReportAsync() {
            using var scope = _serviceProvider.CreateScope();
            var statisticsService = scope.ServiceProvider.GetRequiredService<SalesStatisticsService>();
            var forecastService = scope.ServiceProvider.GetRequiredService<ForecastService>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var weekAgo = DateTime.Now.AddDays(-7);
            var stats = await statisticsService.GetSalesStatisticsAsync(weekAgo, DateTime.Now);
            var forecast = await forecastService.GetSalesForecastAsync();
            var alerts = await forecastService.GetAlertsAsync();

            var subject = $"📊 Еженедельный отчет - {DateTime.Now:dd.MM.yyyy}";
            var body = $@"
                <h2>Еженедельный отчет по продажам</h2>
                <h3>📈 Статистика за неделю:</h3>
                <ul>
                    <li>Выручка: {stats.TotalRevenue:F2} ₽</li>
                    <li>Продано пар: {stats.TotalQuantitySold}</li>
                    <li>Прибыль: {stats.NetProfit:F2} ₽</li>
                </ul>
                
                <h3>🔮 Прогноз на следующую неделю:</h3>
                <ul>
                    <li>Ожидаемая выручка: {forecast.NextWeekRevenue:F2} ₽</li>
                    <li>Тренд: {forecast.TrendPercentage:F1}%</li>
                </ul>
                
                {(alerts.Any() ? $"<h3>⚠️ Важные уведомления:</h3><ul>{string.Join("", alerts.Select(a => $"<li>{a.Message}</li>"))}</ul>" : "")}
                
                <p><small>Автоматический отчет от системы StepLy</small></p>";

            // Отправляем админу (можно настроить email в конфигурации)
            await emailSender.SendEmailAsync("admin@steply.ru", subject, body);
            
            _logger.LogInformation("Еженедельный отчет отправлен");
        }
    }
}