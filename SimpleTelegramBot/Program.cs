using System.Text.Json;

class Program
{
    private static readonly string BotToken = "8468206640:AAFKsz7TklbKeaQbTIsmu__DzU01KK2sx1U";
    private static readonly string ApiUrl = "https://jxpc5n7p-7002.euw.devtunnels.ms/api";
    private static readonly HttpClient httpClient = new();
    private static long lastUpdateId = 0;

    static async Task Main(string[] args)
    {
        Console.WriteLine("🤖 Simple Telegram Bot запущен...");
        
        // Start web server for Mini App
        var webServer = new WebServer("http://localhost:7003/", "wwwroot");
        _ = Task.Run(() => webServer.StartAsync());
        
        while (true)
        {
            try
            {
                await PollUpdates();
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                await Task.Delay(5000);
            }
        }
    }

    static async Task PollUpdates()
    {
        var url = $"https://api.telegram.org/bot{BotToken}/getUpdates?offset={lastUpdateId + 1}&timeout=30";
        
        try
        {
            var response = await httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            if (json.RootElement.GetProperty("ok").GetBoolean())
            {
                var updates = json.RootElement.GetProperty("result").EnumerateArray();
                
                foreach (var update in updates)
                {
                    lastUpdateId = update.GetProperty("update_id").GetInt64();
                    await HandleUpdate(update);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка получения обновлений: {ex.Message}");
        }
    }

    static async Task HandleUpdate(JsonElement update)
    {
        if (update.TryGetProperty("message", out var message))
        {
            var chatId = message.GetProperty("chat").GetProperty("id").GetInt64();
            var text = message.GetProperty("text").GetString() ?? "";
            
            await HandleMessage(chatId, text);
        }
    }

    static async Task HandleMessage(long chatId, string text)
    {
        switch (text)
        {
            case "/start":
                await ShowMenu(chatId);
                break;
            case "🧪 Тест API":
                await TestApi(chatId);
                break;
            case "🛍️ Mini App":
                await ShowMiniApp(chatId);
                break;
            default:
                await SendMessage(chatId, "Используйте кнопки меню 👇");
                break;
        }
    }

    static async Task ShowMenu(long chatId)
    {
        var text = "🤖 Простой Telegram Bot\n\nВыберите действие:";
        
        var keyboard = new
        {
            keyboard = new[]
            {
                new[] { new { text = "🧪 Тест API" }, new { text = "🛍️ Mini App" } }
            },
            resize_keyboard = true,
            persistent = true
        };
        
        await SendMessageWithKeyboard(chatId, text, keyboard);
    }

    static async Task TestApi(long chatId)
    {
        try
        {
            await SendMessage(chatId, "🔄 Тестирую API...");
            
            var response = await httpClient.GetStringAsync($"{ApiUrl}/products");
            var products = JsonDocument.Parse(response);
            
            var text = "✅ API работает!\n\n📦 Товары:\n";
            
            foreach (var product in products.RootElement.EnumerateArray().Take(3))
            {
                var name = product.GetProperty("name").GetString();
                var price = product.GetProperty("finalPrice").GetDouble();
                text += $"• {name} - {price:N0} ₽\n";
            }
            
            await SendMessage(chatId, text);
        }
        catch (Exception ex)
        {
            await SendMessage(chatId, $"❌ Ошибка API: {ex.Message}");
        }
    }

    static async Task ShowMiniApp(long chatId)
    {
        var text = "🛍️ Откройте Mini App для покупок:";
        
        var keyboard = new
        {
            inline_keyboard = new[]
            {
                new[] { new { text = "🛍️ Открыть магазин", web_app = new { url = "https://jxpc5n7p-7003.euw.devtunnels.ms/miniapp.html" } } }
            }
        };
        
        await SendMessageWithInlineKeyboard(chatId, text, keyboard);
    }

    static async Task SendMessage(long chatId, string text)
    {
        var url = $"https://api.telegram.org/bot{BotToken}/sendMessage";
        var payload = new { chat_id = chatId, text = text };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        try
        {
            await httpClient.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка отправки: {ex.Message}");
        }
    }

    static async Task SendMessageWithKeyboard(long chatId, string text, object keyboard)
    {
        var url = $"https://api.telegram.org/bot{BotToken}/sendMessage";
        var payload = new { chat_id = chatId, text = text, reply_markup = keyboard };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        try
        {
            await httpClient.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка отправки: {ex.Message}");
        }
    }

    static async Task SendMessageWithInlineKeyboard(long chatId, string text, object keyboard)
    {
        var url = $"https://api.telegram.org/bot{BotToken}/sendMessage";
        var payload = new { chat_id = chatId, text = text, reply_markup = keyboard };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        try
        {
            await httpClient.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка отправки: {ex.Message}");
        }
    }
}