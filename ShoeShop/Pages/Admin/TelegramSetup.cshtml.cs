using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Services;

namespace ShoeShop.Pages.Admin {
    public class TelegramSetupModel : PageModel {
        private readonly TelegramBotService _telegramService;
        
        public TelegramSetupModel(TelegramBotService telegramService) {
            _telegramService = telegramService;
        }
        
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }
        
        public void OnGet() {
        }
        
        public async Task<IActionResult> OnPostSetWebhookAsync(string webhookUrl) {
            try {
                await _telegramService.SetWebhookAsync(webhookUrl);
                Message = "Webhook успешно установлен!";
                IsSuccess = true;
            }
            catch (Exception ex) {
                Message = $"Ошибка установки webhook: {ex.Message}";
                IsSuccess = false;
            }
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostSendTestAsync(long chatId) {
            try {
                await _telegramService.SendMessageAsync(chatId, 
                    "🎉 Тестовое сообщение от StepLy!\n\n" +
                    "Ваш бот настроен правильно и готов к работе!");
                Message = "Тестовое сообщение отправлено!";
                IsSuccess = true;
            }
            catch (Exception ex) {
                Message = $"Ошибка отправки сообщения: {ex.Message}";
                IsSuccess = false;
            }
            
            return Page();
        }
    }
}