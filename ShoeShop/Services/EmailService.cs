using System.Net.Mail;
using System.Net;

namespace ShoeShop.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAbandonedCartEmailAsync(string email, string customerName, List<string> cartItems)
        {
            var subject = "Вы забыли что-то в корзине! 🛒";
            var body = $@"
                <h2>Привет, {customerName}!</h2>
                <p>Мы заметили, что вы оставили товары в корзине:</p>
                <ul>
                    {string.Join("", cartItems.Select(item => $"<li>{item}</li>"))}
                </ul>
                <p>Не упустите возможность! Завершите покупку со скидкой 10%</p>
                <p>Промокод: <strong>RETURN10</strong></p>
                <a href='https://steply.ru/basket' style='background: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Завершить покупку</a>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendBirthdayEmailAsync(string email, string customerName)
        {
            var subject = "С Днем Рождения! 🎉 Подарок от StepLy";
            var body = $@"
                <h2>С Днем Рождения, {customerName}! 🎂</h2>
                <p>Желаем вам здоровья, счастья и стильной обуви!</p>
                <p>В честь вашего дня рождения дарим скидку 20% на любую пару обуви!</p>
                <p>Промокод: <strong>BIRTHDAY20</strong></p>
                <p>Промокод действует 7 дней.</p>
                <a href='https://steply.ru' style='background: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Выбрать подарок</a>
            ";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("your-email@gmail.com", "your-password"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("noreply@steply.ru", "StepLy"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка отправки email: {ex.Message}");
            }
        }
    }
}