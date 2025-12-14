using Microsoft.AspNetCore.Mvc;
using ShoeShop.Models;
using ShoeShop.Data;

namespace ShoeShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ChatController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Message))
                {
                    return BadRequest(new { success = false, error = "Пустое сообщение" });
                }

                var botResponse = GetBotResponse(request.Message);
                var isAutoResponse = !botResponse.Contains("Передаю ваш запрос") && !botResponse.Contains("Не совсем понял");

                var message = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId ?? "anonymous",
                    UserName = request.UserName ?? "Гость",
                    Message = request.Message,
                    CreatedAt = DateTime.Now,
                    IsAnswered = isAutoResponse,
                    IsAutoResponse = isAutoResponse,
                    Response = isAutoResponse ? botResponse : null,
                    RespondedBy = isAutoResponse ? "Бот" : null,
                    RespondedAt = isAutoResponse ? DateTime.Now : null
                };

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();
                
                return Ok(new { 
                    success = true, 
                    response = botResponse,
                    messageId = message.Id 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("unread")]
        public IActionResult GetUnreadMessages()
        {
            var unread = _context.ChatMessages
                .Where(m => !m.IsAnswered && !m.IsAutoResponse && !m.IsClosed)
                .OrderBy(m => m.CreatedAt)
                .ToList();
            return Ok(unread);
        }

        [HttpPost("respond")]
        public async Task<IActionResult> RespondToMessage([FromBody] RespondRequest request)
        {
            var message = _context.ChatMessages
                .FirstOrDefault(m => m.Id.ToString() == request.MessageId);
            
            if (message != null)
            {
                message.Response = request.Response;
                message.RespondedBy = request.RespondedBy;
                message.RespondedAt = DateTime.Now;
                message.IsAnswered = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }

        [HttpGet("history/{userId}")]
        public IActionResult GetChatHistory(string userId)
        {
            var messages = _context.ChatMessages
                .Where(m => m.UserId == userId && !m.IsClosed)
                .OrderBy(m => m.CreatedAt)
                .ToList();
            return Ok(messages);
        }

        [HttpPost("close")]
        public async Task<IActionResult> CloseChat([FromBody] CloseChatRequest request)
        {
            try
            {
                var messages = _context.ChatMessages
                    .Where(m => m.UserId == request.UserId)
                    .ToList();
                
                foreach (var message in messages)
                {
                    message.IsClosed = true;
                }
                
                await _context.SaveChangesAsync();
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        private string GetBotResponse(string message)
        {
            var msg = message.ToLower();
            
            // Проверяем запрос на связь с человеком
            if (msg.Contains("человек") || msg.Contains("оператор") || msg.Contains("консультант") || 
                msg.Contains("сотрудник") || msg.Contains("менеджер") || msg.Contains("связать"))
            {
                return "Передаю ваш запрос консультанту. Он ответит вам в ближайшее время.";
            }
            
            // Ответы бота на частые вопросы
            if (msg.Contains("размер"))
                return "👟 Для выбора размера рекомендуем измерить длину стопы. У нас есть размеры от 36 до 45. Нужна помощь с конкретной моделью?";
            
            if (msg.Contains("доставка"))
                return "🚚 Доставка по Москве - 300₽, по России - от 500₽. 🎁 Бесплатная доставка при заказе от 5000₽. Срок доставки 1-3 дня.";
            
            if (msg.Contains("оплата") || msg.Contains("платить"))
                return "💳 Принимаем оплату: картой онлайн, наличными при получении, переводом на карту. Оплата безопасна и защищена.";
            
            if (msg.Contains("скидка") || msg.Contains("акция") || msg.Contains("промокод"))
                return "🎉 Сейчас действует скидка до 30% на зимнюю коллекцию! Также есть промокоды для постоянных клиентов.";
            
            if (msg.Contains("возврат") || msg.Contains("обмен"))
                return "🔄 Возврат и обмен в течение 14 дней. Обувь должна быть в оригинальной упаковке и не ношеной.";
            
            if (msg.Contains("качество") || msg.Contains("материал"))
                return "✨ Мы работаем только с проверенными поставщиками. Вся обувь из натуральных материалов с гарантией качества.";
            
            if (msg.Contains("контакт") || msg.Contains("телефон") || msg.Contains("адрес"))
                return "📞 Контакты: +7 (999) 123-45-67\n📧 Email: info@steply.ru\n📍 Адрес: г. Москва, ул. Примерная, д. 1\n🕰️ Режим работы: Пн-Вс 9:00-21:00";
            
            // Общие приветствия
            if (msg.Contains("привет") || msg.Contains("здравствуй") || msg.Contains("добрый"))
                return "👋 Привет! Я виртуальный консультант StepLy. Могу помочь с выбором обуви, рассказать о доставке и оплате. Что вас интересует?";
            
            // Ответ по умолчанию - передача консультанту
            return "🤔 Не совсем понял ваш вопрос. Передаю вас нашему консультанту - он ответит в ближайшее время! 😊";
        }
    }

    public class SendMessageRequest
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class RespondRequest
    {
        public string MessageId { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string RespondedBy { get; set; } = string.Empty;
    }

    public class CloseChatRequest
    {
        public string UserId { get; set; } = string.Empty;
    }
}