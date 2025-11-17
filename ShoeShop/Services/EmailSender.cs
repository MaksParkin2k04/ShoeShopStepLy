using Microsoft.AspNetCore.Identity.UI.Services;

namespace ShoeShop.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Заглушка для отправки email в режиме разработки
            // В продакшене здесь должна быть реальная отправка email
            return Task.CompletedTask;
        }
    }
}