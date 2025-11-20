using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace LoadTest
{
    public class LoadTestRunner
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly List<string> _testUrls;

        public LoadTestRunner(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            _testUrls = new List<string>
            {
                "/",
                "/Products",
                "/Products?page=2",
                "/Categories"
            };
        }

        public async Task<LoadTestResult> RunTest(int concurrentUsers, int durationSeconds)
        {
            var result = new LoadTestResult
            {
                ConcurrentUsers = concurrentUsers,
                DurationSeconds = durationSeconds,
                StartTime = DateTime.Now
            };

            var tasks = new List<Task>();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));
            var requestCount = 0;
            var errorCount = 0;
            var responseTimes = new List<long>();

            Console.WriteLine($"Запуск теста: {concurrentUsers} пользователей, {durationSeconds} секунд");

            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var random = new Random();
                    while (!cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var url = _testUrls[random.Next(_testUrls.Count)];
                            var stopwatch = Stopwatch.StartNew();
                            
                            var response = await _httpClient.GetAsync(_baseUrl + url, cancellationTokenSource.Token);
                            stopwatch.Stop();
                            
                            Interlocked.Increment(ref requestCount);
                            
                            lock (responseTimes)
                            {
                                responseTimes.Add(stopwatch.ElapsedMilliseconds);
                            }
                            
                            if (!response.IsSuccessStatusCode)
                            {
                                Interlocked.Increment(ref errorCount);
                            }
                            
                            await Task.Delay(random.Next(100, 1000), cancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception)
                        {
                            Interlocked.Increment(ref errorCount);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            result.EndTime = DateTime.Now;
            result.TotalRequests = requestCount;
            result.ErrorCount = errorCount;
            result.RequestsPerSecond = (double)requestCount / durationSeconds;
            
            if (responseTimes.Count > 0)
            {
                responseTimes.Sort();
                result.AverageResponseTime = responseTimes.Sum() / responseTimes.Count;
                result.MedianResponseTime = responseTimes[responseTimes.Count / 2];
                result.MaxResponseTime = responseTimes[responseTimes.Count - 1];
                result.MinResponseTime = responseTimes[0];
            }

            return result;
        }
    }

    public class LoadTestResult
    {
        public int ConcurrentUsers { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalRequests { get; set; }
        public int ErrorCount { get; set; }
        public double RequestsPerSecond { get; set; }
        public double AverageResponseTime { get; set; }
        public long MedianResponseTime { get; set; }
        public long MaxResponseTime { get; set; }
        public long MinResponseTime { get; set; }
        public double ErrorRate => TotalRequests > 0 ? (double)ErrorCount / TotalRequests * 100 : 0;

        public void PrintResults()
        {
            Console.WriteLine($"\nРезультаты для {ConcurrentUsers} пользователей:");
            Console.WriteLine($"Запросов: {TotalRequests}, Ошибок: {ErrorCount} ({ErrorRate:F1}%)");
            Console.WriteLine($"RPS: {RequestsPerSecond:F1}, Время ответа: {AverageResponseTime:F0}мс");
            
            if (ErrorRate < 1 && AverageResponseTime < 1000)
                Console.WriteLine("✅ ОТЛИЧНО");
            else if (ErrorRate < 5 && AverageResponseTime < 3000)
                Console.WriteLine("⚠️ ХОРОШО");
            else
                Console.WriteLine("❌ ПЛОХО");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== НАГРУЗОЧНОЕ ТЕСТИРОВАНИЕ ===");
            
            string baseUrl = "https://localhost:7139";
            var loadTester = new LoadTestRunner(baseUrl);
            
            try
            {
                var result1 = await loadTester.RunTest(10, 30);
                result1.PrintResults();
                
                await Task.Delay(3000);
                
                var result2 = await loadTester.RunTest(50, 30);
                result2.PrintResults();
                
                await Task.Delay(3000);
                
                var result3 = await loadTester.RunTest(100, 30);
                result3.PrintResults();
                
                var maxRps = Math.Max(Math.Max(result1.RequestsPerSecond, result2.RequestsPerSecond), result3.RequestsPerSecond);
                var estimatedMaxUsers = (int)(maxRps * 3);
                
                Console.WriteLine($"\nМаксимальная производительность: {maxRps:F1} RPS");
                Console.WriteLine($"Оценка пользователей: {estimatedMaxUsers:N0}");
                
                if (estimatedMaxUsers >= 1000)
                    Console.WriteLine("✅ Готов для 1000+ пользователей!");
                else
                    Console.WriteLine($"⚠️ Выдержит около {estimatedMaxUsers} пользователей");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            
            Console.ReadKey();
        }
    }
}