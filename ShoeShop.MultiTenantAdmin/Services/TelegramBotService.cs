using System.Text.Json;

namespace ShoeShop.MultiTenantAdmin.Services {
    public class TelegramBotService {
        private readonly string _botToken;
        private readonly HttpClient _httpClient;
        
        public TelegramBotService(IConfiguration configuration, HttpClient httpClient) {
            _botToken = configuration["Telegram:BotToken"] ?? "8468206640:AAFKsz7TklbKeaQbTIsmu__DzU01KK2sx1U";
            _httpClient = httpClient;
        }
        
        public async Task SendMessageAsync(long chatId, string message) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new {
                chat_id = chatId,
                text = message,
                parse_mode = "Markdown"
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            await _httpClient.PostAsync(url, content);
        }
        
        public async Task SendMessageWithKeyboardAsync(long chatId, string message, object keyboard) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new {
                chat_id = chatId,
                text = message,
                parse_mode = "Markdown",
                reply_markup = keyboard
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            await _httpClient.PostAsync(url, content);
        }
        
        public async Task SendMessageWithInlineKeyboardAsync(long chatId, string message, object keyboard) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new {
                chat_id = chatId,
                text = message,
                parse_mode = "Markdown",
                reply_markup = keyboard
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            await _httpClient.PostAsync(url, content);
        }
        
        public async Task SetWebhookAsync(string webhookUrl) {
            var url = $"https://api.telegram.org/bot{_botToken}/setWebhook";
            var payload = new { url = webhookUrl };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            await _httpClient.PostAsync(url, content);
        }
    }
}
