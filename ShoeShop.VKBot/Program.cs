using System.Text.Json;
using ShoeShop.Shared.DTOs;

namespace ShoeShop.VKBot;

class Program {
    private static readonly string VkToken = "YOUR_VK_TOKEN_HERE";
    private static readonly string ApiBaseUrl = "https://localhost:7001/api";
    private static readonly HttpClient httpClient = new();
    
    static async Task Main(string[] args) {
        Console.WriteLine("VK Bot запущен...");
        
        // TODO: Реализовать VK Bot API
        while (true) {
            try {
                // Получение обновлений от VK
                await Task.Delay(1000);
            }
            catch (Exception ex) {
                Console.WriteLine($"Ошибка: {ex.Message}");
                await Task.Delay(5000);
            }
        }
    }
    
    static async Task<List<ProductDto>?> GetProductsFromApi() {
        try {
            var response = await httpClient.GetStringAsync($"{ApiBaseUrl}/products");
            return JsonSerializer.Deserialize<List<ProductDto>>(response);
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка получения товаров: {ex.Message}");
            return null;
        }
    }
    
    static async Task<OrderDto?> CreateOrderViaApi(OrderCreateDto order) {
        try {
            var json = JsonSerializer.Serialize(order);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{ApiBaseUrl}/orders", content);
            
            if (response.IsSuccessStatusCode) {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrderDto>(responseJson);
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка создания заказа: {ex.Message}");
        }
        return null;
    }
}
