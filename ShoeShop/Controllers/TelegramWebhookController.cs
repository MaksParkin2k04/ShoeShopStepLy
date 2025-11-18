using Microsoft.AspNetCore.Mvc;
using ShoeShop.Services;
using System.Text.Json;

namespace ShoeShop.Controllers {
    [ApiController]
    [Route("api/telegram")]
    public class TelegramWebhookController : ControllerBase {
        private readonly TelegramBotHandler _botHandler;
        private readonly ILogger<TelegramWebhookController> _logger;
        
        public TelegramWebhookController(TelegramBotHandler botHandler, ILogger<TelegramWebhookController> logger) {
            _botHandler = botHandler;
            _logger = logger;
        }
        
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] JsonElement update) {
            try {
                _logger.LogInformation($"Received update: {update}");
                
                // Простая обработка /start
                if (update.TryGetProperty("message", out var message)) {
                    var chatId = message.GetProperty("chat").GetProperty("id").GetInt64();
                    var text = message.GetProperty("text").GetString();
                    
                    if (text == "/start") {
                        var botToken = "8468206640:AAFKsz7TklbKeaQbTIsmu__DzU01KK2sx1U";
                        var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
                        
                        var payload = new {
                            chat_id = chatId,
                            text = "🛍️ Добро пожаловать в StepLy! Бот работает!"
                        };
                        
                        var json = System.Text.Json.JsonSerializer.Serialize(payload);
                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        
                        using var client = new HttpClient();
                        await client.PostAsync(url, content);
                    }
                }
                
                return Ok();
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error processing webhook");
                return Ok($"Error: {ex.Message}");
            }
        }
        
        [HttpGet("test")]
        public IActionResult Test() {
            return Ok("Webhook endpoint is working");
        }
        
        [HttpGet("webhook")]
        public IActionResult WebhookGet() {
            return Ok($"Webhook is ready. Time: {DateTime.Now}");
        }
    }
}