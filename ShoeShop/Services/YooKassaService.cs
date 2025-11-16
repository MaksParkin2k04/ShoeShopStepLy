using System.Text;
using System.Text.Json;

namespace ShoeShop.Services
{
    public class YooKassaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _shopId;
        private readonly string _secretKey;

        public YooKassaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _shopId = configuration["YooKassa:ShopId"] ?? "test_shop_id";
            _secretKey = configuration["YooKassa:SecretKey"] ?? "test_secret_key";
            
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_shopId}:{_secretKey}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            _httpClient.BaseAddress = new Uri("https://api.yookassa.ru/v3/");
        }

        public async Task<YooKassaPaymentResponse> CreatePaymentAsync(decimal amount, string description, string returnUrl, Guid orderId)
        {
            var payment = new
            {
                amount = new { value = amount.ToString("F2"), currency = "RUB" },
                confirmation = new { type = "redirect", return_url = returnUrl },
                description = description,
                metadata = new { order_id = orderId.ToString() }
            };

            var json = JsonSerializer.Serialize(payment);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("Idempotence-Key", Guid.NewGuid().ToString());

            var response = await _httpClient.PostAsync("payments", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<YooKassaPaymentResponse>(responseJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            }

            throw new Exception($"Payment creation failed: {responseJson}");
        }
    }

    public class YooKassaPaymentResponse
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public YooKassaConfirmation Confirmation { get; set; }
    }

    public class YooKassaConfirmation
    {
        public string Type { get; set; }
        public string ConfirmationUrl { get; set; }
    }
}