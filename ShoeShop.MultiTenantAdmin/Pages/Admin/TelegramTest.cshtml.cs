using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.Services;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin {
    public class TelegramTestModel : PageModel {
        private readonly TelegramBotService _telegramService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        
        public TelegramTestModel(TelegramBotService telegramService, HttpClient httpClient, IConfiguration configuration) {
            _telegramService = telegramService;
            _httpClient = httpClient;
            _configuration = configuration;
        }
        
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }
        
        public void OnGet() {
        }
        
        public async Task<IActionResult> OnPostCheckWebhookAsync() {
            try {
                var botToken = _configuration["Telegram:BotToken"];
                var url = $"https://api.telegram.org/bot{botToken}/getWebhookInfo";
                var response = await _httpClient.GetStringAsync(url);
                
                Message = $"Webhook info: {response}";
                IsSuccess = true;
            }
            catch (Exception ex) {
                Message = $"Ошибка: {ex.Message}";
                IsSuccess = false;
            }
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostSendMessageAsync(long chatId, string message) {
            try {
                await _telegramService.SendMessageAsync(chatId, message);
                Message = "Сообщение отправлено!";
                IsSuccess = true;
            }
            catch (Exception ex) {
                Message = $"Ошибка отправки: {ex.Message}";
                IsSuccess = false;
            }
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostSetWebhookAsync(string webhookUrl) {
            try {
                await _telegramService.SetWebhookAsync(webhookUrl);
                Message = "Webhook установлен!";
                IsSuccess = true;
            }
            catch (Exception ex) {
                Message = $"Ошибка установки webhook: {ex.Message}";
                IsSuccess = false;
            }
            
            return Page();
        }
    }
}
