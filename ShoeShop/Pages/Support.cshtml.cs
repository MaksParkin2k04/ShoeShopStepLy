using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Pages
{
    public class SupportModel : PageModel
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<SupportModel> _logger;

        public SupportModel(ApplicationContext context, ILogger<SupportModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<ChatMessage> Messages { get; set; } = new();
        public string CurrentUserId { get; set; } = "";

        public async Task OnGetAsync()
        {
            // Получаем или создаем ID пользователя
            CurrentUserId = Request.Cookies["SupportUserId"] ?? "";
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                CurrentUserId = "user_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Response.Cookies.Append("SupportUserId", CurrentUserId, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = false
                });
            }

            // Загружаем историю сообщений
            Messages = await _context.ChatMessages
                .Where(m => m.UserId == CurrentUserId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostSendMessageAsync(string message)
        {
            try
            {
                CurrentUserId = Request.Cookies["SupportUserId"] ?? "";
                if (string.IsNullOrEmpty(CurrentUserId))
                {
                    return BadRequest("User ID not found");
                }

                // Проверяем, есть ли неотвеченные сообщения (ожидаем консультанта)
                var hasUnansweredMessages = await _context.ChatMessages
                    .AnyAsync(m => m.UserId == CurrentUserId && !m.IsAnswered && !m.IsClosed);

                // Проверяем, был ли уже ответ от оператора (не бота) в этом чате
                var hasOperatorResponse = await _context.ChatMessages
                    .AnyAsync(m => m.UserId == CurrentUserId && !string.IsNullOrEmpty(m.RespondedBy) && m.RespondedBy != "Бот" && !m.IsClosed);

                string botResponse = null;
                bool isAutoResponse = false;

                // Бот отвечает только если нет ожидающих сообщений И не было ответов от оператора
                if (!hasUnansweredMessages && !hasOperatorResponse)
                {
                    botResponse = GetBotResponse(message);
                    isAutoResponse = !botResponse.Contains("Передаю ваш запрос") && !botResponse.Contains("Не совсем понял");
                }

                var chatMessage = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUserId,
                    UserName = User.Identity?.Name ?? "Пользователь сайта",
                    Message = message,
                    CreatedAt = DateTime.Now,
                    IsAnswered = isAutoResponse,
                    IsAutoResponse = isAutoResponse,
                    Response = isAutoResponse ? botResponse : null,
                    RespondedBy = isAutoResponse ? "Бот" : null,
                    RespondedAt = isAutoResponse ? DateTime.Now : null,
                    IsClosed = false
                };

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                return RedirectToPage();
            }
        }

        private string GetBotResponse(string message)
        {
            var msg = message.ToLower().Trim();
            
            if (msg.Contains("человек") || msg.Contains("оператор") || msg.Contains("консультант") || 
                msg.Contains("сотрудник") || msg.Contains("менеджер") || msg.Contains("связать") ||
                msg.Contains("живой") || msg.Contains("реальный"))
            {
                return "👤 Передаю ваш запрос консультанту. Он ответит вам в ближайшее время. Обычно это занимает 2-5 минут.";
            }
            
            if (msg.Contains("привет") || msg.Contains("здравствуй") || msg.Contains("добрый") || msg == "hi" || msg == "hello")
                return "👋 Привет! Я виртуальный консультант StepLy. Помогу выбрать обувь, расскажу о доставке, оплате и акциях. Что вас интересует?";
            
            if (msg.Contains("спасибо") || msg.Contains("благодар"))
                return "😊 Пожалуйста! Рад был помочь. Если есть еще вопросы - обращайтесь!";
            
            if (msg.Contains("размер") || msg.Contains("размерн"))
            {
                if (msg.Contains("таблица") || msg.Contains("сетка"))
                    return "📏 Таблица размеров:\n\n36 = 23 см\n37 = 23.5 см\n38 = 24 см\n39 = 24.5 см\n40 = 25 см\n41 = 25.5 см\n42 = 26 см\n43 = 26.5 см\n44 = 27 см\n45 = 27.5 см\n\n💡 Измерьте длину стопы от пятки до большого пальца";
                
                return "👟 Для выбора размера измерьте длину стопы линейкой. У нас размеры от 36 до 45. Хотите таблицу размеров?";
            }
            
            if (msg.Contains("доставка") || msg.Contains("доставить") || msg.Contains("привез"))
            {
                if (msg.Contains("срок") || msg.Contains("когда") || msg.Contains("быстро"))
                    return "⚡ Сроки доставки:\n🏃‍♂️ Экспресс (Москва) - в день заказа, 800₽\n🚚 Обычная (Москва) - 1-2 дня, 300₽\n📦 По России - 2-5 дней, от 500₽\n🎁 Бесплатно при заказе от 5000₽";
                
                if (msg.Contains("стоимость") || msg.Contains("цена") || msg.Contains("сколько"))
                    return "💰 Стоимость доставки:\n🏠 Москва - 300₽\n🌍 Россия - от 500₽\n🎁 БЕСПЛАТНО при заказе от 5000₽\n⚡ Экспресс-доставка +500₽";
                
                return "🚚 Доставляем по всей России! Москва - 300₽ (1-2 дня), регионы - от 500₽ (2-5 дней). Бесплатная доставка от 5000₽!";
            }
            
            if (msg.Contains("оплата") || msg.Contains("платить") || msg.Contains("заплатить") || msg.Contains("карт"))
            {
                return "💳 Способы оплаты:\n\n💰 Наличными курьеру\n💳 Картой онлайн (Visa, MasterCard, МИР)\n📱 СБП (Система быстрых платежей)\n🏦 Переводом на карту\n\n🔒 Все платежи защищены SSL-шифрованием";
            }
            
            if (msg.Contains("скидка") || msg.Contains("акция") || msg.Contains("промокод") || msg.Contains("распродажа"))
            {
                return "🎉 Актуальные акции:\n\n❄️ Зимняя распродажа - до 50%\n🎁 Промокод FIRST10 - 10% новым клиентам\n👥 При покупке 2 пар - скидка 15%\n🎂 В день рождения - скидка 20%\n\n📱 Подпишитесь на рассылку для эксклюзивных предложений!";
            }
            
            if (msg.Contains("возврат") || msg.Contains("обмен") || msg.Contains("вернуть"))
            {
                return "🔄 Возврат и обмен:\n\n✅ В течение 14 дней\n📦 В оригинальной упаковке\n👟 Без следов носки\n🧾 При наличии чека\n\n📞 Для оформления звоните: +7 (999) 123-45-67";
            }
            
            if (msg.Contains("качество") || msg.Contains("материал") || msg.Contains("кожа") || msg.Contains("подошва"))
            {
                return "✨ О качестве:\n\n🏭 Работаем с проверенными брендами\n🐄 Натуральная кожа и замша\n💪 Прочные подошвы (резина, полиуретан)\n🛡️ Гарантия качества 6 месяцев\n🔍 Каждая пара проходит контроль";
            }
            
            if (msg.Contains("контакт") || msg.Contains("телефон") || msg.Contains("адрес") || msg.Contains("где находит"))
            {
                return "📞 Наши контакты:\n\n☎️ +7 (999) 123-45-67\n📧 info@steply.ru\n📍 г. Москва, ул. Примерная, д. 1\n🕰️ Пн-Вс: 9:00-21:00\n\n🌐 Сайт: steply.ru\n📱 Telegram: @steply_bot";
            }
            
            if (msg.Contains("кроссовки") || msg.Contains("ботинки") || msg.Contains("туфли") || msg.Contains("обувь"))
            {
                if (msg.Contains("новинки") || msg.Contains("новые"))
                    return "🆕 Новинки этой недели:\n\n👟 Nike Air Max 270 - 8990₽\n⚡ Adidas Ultraboost 22 - 12990₽\n🔥 Puma RS-X - 7490₽\n\n🛍️ Посмотреть все новинки можно в каталоге на сайте!";
                
                if (msg.Contains("популярн") || msg.Contains("хит") || msg.Contains("лучш"))
                    return "🔥 Хиты продаж:\n\n1️⃣ Nike Air Force 1 - 9990₽\n2️⃣ Adidas Stan Smith - 6990₽\n3️⃣ Converse Chuck Taylor - 4990₽\n\n⭐ Эти модели покупают чаще всего!";
                
                return "👟 У нас большой выбор обуви: кроссовки, ботинки, туфли от ведущих брендов. Что именно ищете?";
            }
            
            if (msg.Contains("nike") || msg.Contains("найк"))
                return "✅ Nike в наличии! Популярные модели: Air Force 1, Air Max, Dunk, Jordan. Цены от 6990₽. Хотите посмотреть конкретную модель?";
            
            if (msg.Contains("adidas") || msg.Contains("адидас"))
                return "✅ Adidas в ассортименте! Есть: Stan Smith, Ultraboost, Gazelle, Superstar. Цены от 5990₽. Какая модель интересует?";
            
            if (msg.Contains("цена") || msg.Contains("стоимость") || msg.Contains("сколько стоит"))
            {
                return "💰 Наши цены:\n\n👟 Кроссовки: от 3990₽ до 25990₽\n👞 Ботинки: от 5990₽ до 18990₽\n👠 Туфли: от 4990₽ до 15990₽\n\n🏷️ Часто действуют скидки до 50%!";
            }
            
            if (msg.Contains("помощь") || msg.Contains("помоги") || msg.Contains("как"))
            {
                return "🆘 Чем могу помочь:\n\n👟 Выбор размера и модели\n🚚 Информация о доставке\n💳 Способы оплаты\n🎁 Актуальные акции\n🔄 Возврат и обмен\n\nЗадавайте любые вопросы!";
            }
            
            if (msg.Length <= 3)
            {
                return "🤔 Не понял... Можете написать вопрос подробнее? Я помогу с выбором обуви!";
            }
            
            return "🤖 Извините, не совсем понял ваш вопрос. Попробуйте спросить:\n\n• О размерах и моделях\n• О доставке и оплате\n• Об акциях и скидках\n\nИли напишите \"помощь\" для списка команд. Если нужна персональная консультация - передам вас специалисту!";
        }

        public async Task<IActionResult> OnPostCloseChatAsync()
        {
            try
            {
                CurrentUserId = Request.Cookies["SupportUserId"] ?? "";
                if (string.IsNullOrEmpty(CurrentUserId))
                {
                    return BadRequest("User ID not found");
                }

                // Добавляем финальное сообщение от системы
                var finalMessage = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUserId,
                    UserName = "Система",
                    Message = "✅ Вопрос решен. Спасибо за обращение в службу поддержки StepLy! Если у вас возникнут новые вопросы, мы всегда готовы помочь.",
                    CreatedAt = DateTime.Now,
                    IsAnswered = true,
                    IsAutoResponse = false,
                    Response = null,
                    RespondedBy = "Система",
                    RespondedAt = DateTime.Now,
                    IsClosed = true
                };

                _context.ChatMessages.Add(finalMessage);

                // Закрываем все предыдущие сообщения
                var messages = await _context.ChatMessages
                    .Where(m => m.UserId == CurrentUserId && !m.IsClosed)
                    .ToListAsync();

                foreach (var message in messages)
                {
                    message.IsClosed = true;
                    message.IsAnswered = true;
                }

                await _context.SaveChangesAsync();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error closing chat: {ex.Message}");
                return RedirectToPage();
            }
        }
    }
}