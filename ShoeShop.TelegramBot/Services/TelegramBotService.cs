using System.Text.Json;
using ShoeShop.Shared.DTOs;
using ShoeShop.TelegramBot.Models;

namespace ShoeShop.TelegramBot.Services;

public class TelegramBotService {
    private readonly string _botToken = "8468206640:AAFKsz7TklbKeaQbTIsmu__DzU01KK2sx1U";
    private readonly string _apiBaseUrl = "https://jxpc5n7p-7002.euw.devtunnels.ms/api";
    private readonly HttpClient _httpClient = new();
    private long _lastUpdateId = 0;
    
    private static readonly Dictionary<long, UserSession> _userSessions = new();
    
    public async Task StartAsync() {
        Console.WriteLine("🤖 Telegram магазин запущен...");
        
        while (true) {
            try {
                await PollUpdates();
                await Task.Delay(1000);
            }
            catch (Exception ex) {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                await Task.Delay(5000);
            }
        }
    }
    
    private async Task PollUpdates() {
        var url = $"https://api.telegram.org/bot{_botToken}/getUpdates?offset={_lastUpdateId + 1}&timeout=30";
        
        try {
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            if (json.RootElement.GetProperty("ok").GetBoolean()) {
                var updates = json.RootElement.GetProperty("result").EnumerateArray();
                
                foreach (var update in updates) {
                    _lastUpdateId = update.GetProperty("update_id").GetInt64();
                    await HandleUpdate(update);
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Ошибка получения обновлений: {ex.Message}");
        }
    }
    
    private async Task HandleUpdate(JsonElement update) {
        if (update.TryGetProperty("callback_query", out var callback)) {
            await HandleCallback(callback);
        } else if (update.TryGetProperty("message", out var message)) {
            var chatId = message.GetProperty("chat").GetProperty("id").GetInt64();
            var text = message.GetProperty("text").GetString() ?? "";
            
            await HandleMessage(chatId, text);
        }
    }
    
    private async Task HandleMessage(long chatId, string text) {
        if (!_userSessions.ContainsKey(chatId)) {
            _userSessions[chatId] = new UserSession();
        }
        
        var session = _userSessions[chatId];
        
        if (session.State != UserState.None) {
            await HandleUserInput(chatId, text);
            return;
        }
        
        switch (text) {
            case "/start":
                await ShowWelcome(chatId);
                break;
            case "🛍️ Каталог":
                await ShowProducts(chatId);
                break;
            case "🛒 Корзина":
                await ShowCart(chatId);
                break;
            case "📦 Мои заказы":
                await ShowOrders(chatId);
                break;
            case "ℹ️ О магазине":
                await ShowAbout(chatId);
                break;
            default:
                await SendMessage(chatId, "Используйте меню для навигации 👇");
                break;
        }
    }
    
    private async Task ShowWelcome(long chatId) {
        var text = "👋 **Добро пожаловать в StepLy!**\n\n";
        text += "🏪 Интернет-магазин кроссовок\n";
        text += "👟 Оригинальная продукция\n";
        text += "🚚 Быстрая доставка\n";
        text += "💳 Удобная оплата\n\n";
        text += "Выберите способ покупок:";
        
        var inlineKeyboard = new {
            inline_keyboard = new object[][] {
                new object[] { new { text = "🛍️ Открыть магазин", web_app = new { url = "https://jxpc5n7p-7003.euw.devtunnels.ms/miniapp" } } },
                new object[] { new { text = "💬 Покупки в чате", callback_data = "chat_shopping" } }
            }
        };
        
        var replyKeyboard = new {
            keyboard = new[] {
                new[] { new { text = "🛍️ Каталог" }, new { text = "🛒 Корзина" } },
                new[] { new { text = "📦 Мои заказы" }, new { text = "ℹ️ О магазине" } }
            },
            resize_keyboard = true,
            persistent = true
        };
        
        await SendMessageWithInlineKeyboard(chatId, text, inlineKeyboard);
        await SendMessageWithKeyboard(chatId, "Или используйте меню ниже:", replyKeyboard);
    }
    
    private async Task ShowProducts(long chatId) {
        try {
            var response = await _httpClient.GetStringAsync($"{_apiBaseUrl}/products");
            var products = JsonSerializer.Deserialize<List<ProductDto>>(response);
            
            if (products?.Any() == true) {
                var text = "👟 **Наш каталог:**\n\n";
                var buttons = new List<object[]>();
                
                foreach (var product in products.Take(10)) {
                    text += $"🔸 **{product.Name}**\n";
                    text += $"💰 {product.FinalPrice:N0} ₽";
                    if (product.SalePrice.HasValue) {
                        text += $" ~~{product.Price:N0} ₽~~";
                    }
                    text += $"\n📝 {product.Description}\n\n";
                    
                    buttons.Add(new[] {
                        new { text = $"👀 {product.Name}", callback_data = $"product_{product.Id}" }
                    });
                }
                
                var keyboard = new { inline_keyboard = buttons.ToArray() };
                await SendMessageWithInlineKeyboard(chatId, text, keyboard);
            } else {
                await SendMessage(chatId, "😔 Товары временно недоступны");
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Ошибка получения товаров: {ex.Message}");
            await SendMessage(chatId, "❌ Ошибка загрузки каталога");
        }
    }
    
    private async Task ShowCart(long chatId) {
        var session = _userSessions[chatId];
        
        if (!session.Cart.Any()) {
            await SendMessage(chatId, "🛒 Корзина пуста\n\nДобавьте товары из каталога!");
            return;
        }
        
        var text = "🛒 **Ваша корзина:**\n\n";
        var total = 0.0;
        
        foreach (var item in session.Cart) {
            text += $"• {item.Name} (размер {item.Size})\n";
            text += $"  {item.Quantity} шт. × {item.Price:N0} ₽ = {item.Price * item.Quantity:N0} ₽\n\n";
            total += item.Price * item.Quantity;
        }
        
        text += $"💰 **Итого: {total:N0} ₽**";
        
        var keyboard = new {
            inline_keyboard = new[] {
                new[] { new { text = "📋 Оформить заказ", callback_data = "checkout" } },
                new[] { new { text = "🗑️ Очистить корзину", callback_data = "clear_cart" } }
            }
        };
        
        await SendMessageWithInlineKeyboard(chatId, text, keyboard);
    }
    
    private async Task HandleCallback(JsonElement callback) {
        var chatId = callback.GetProperty("message").GetProperty("chat").GetProperty("id").GetInt64();
        var data = callback.GetProperty("data").GetString() ?? "";
        
        var parts = data.Split('_');
        if (parts.Length < 2) return;
        
        switch (parts[0]) {
            case "product":
                await ShowProductDetail(chatId, Guid.Parse(parts[1]));
                break;
            case "add":
                await AddToCart(chatId, Guid.Parse(parts[1]), int.Parse(parts[2]));
                break;
            case "checkout":
                await StartCheckout(chatId);
                break;
            case "clear":
                if (parts[1] == "cart") await ClearCart(chatId);
                break;
            case "chat":
                if (parts[1] == "shopping") await ShowChatShopping(chatId);
                break;
        }
    }
    
    private async Task ShowProductDetail(long chatId, Guid productId) {
        try {
            var response = await _httpClient.GetStringAsync($"{_apiBaseUrl}/products/{productId}");
            var product = JsonSerializer.Deserialize<ProductDto>(response);
            
            if (product != null) {
                var text = $"👟 **{product.Name}**\n\n";
                text += $"💰 **{product.FinalPrice:N0} ₽**";
                if (product.SalePrice.HasValue) {
                    text += $" ~~{product.Price:N0} ₽~~";
                }
                text += $"\n\n📝 {product.Content}\n\n";
                text += "👟 **Доступные размеры:**";
                
                var buttons = new List<object[]>();
                
                foreach (var size in product.Sizes) {
                    buttons.Add(new[] {
                        new { text = $"Размер {size}", callback_data = $"add_{productId}_{size}" }
                    });
                }
                
                var keyboard = new { inline_keyboard = buttons.ToArray() };
                await SendMessageWithInlineKeyboard(chatId, text, keyboard);
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Ошибка получения товара: {ex.Message}");
            await SendMessage(chatId, "❌ Товар не найден");
        }
    }
    
    private async Task AddToCart(long chatId, Guid productId, int size) {
        try {
            var response = await _httpClient.GetStringAsync($"{_apiBaseUrl}/products/{productId}");
            var product = JsonSerializer.Deserialize<ProductDto>(response);
            
            if (product != null) {
                var session = _userSessions[chatId];
                var existingItem = session.Cart.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
                
                if (existingItem != null) {
                    existingItem.Quantity++;
                } else {
                    session.Cart.Add(new CartItem {
                        ProductId = productId,
                        Name = product.Name,
                        Price = product.FinalPrice,
                        Size = size,
                        Quantity = 1
                    });
                }
                
                await SendMessage(chatId, $"✅ {product.Name} (размер {size}) добавлен в корзину!");
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Ошибка добавления в корзину: {ex.Message}");
            await SendMessage(chatId, "❌ Ошибка добавления товара");
        }
    }
    
    private async Task StartCheckout(long chatId) {
        var session = _userSessions[chatId];
        session.State = UserState.WaitingName;
        await SendMessage(chatId, "👤 Введите ваше имя:");
    }
    
    private async Task HandleUserInput(long chatId, string text) {
        var session = _userSessions[chatId];
        
        switch (session.State) {
            case UserState.WaitingName:
                session.OrderData.Name = text;
                session.State = UserState.WaitingPhone;
                await SendMessage(chatId, "📱 Введите номер телефона:");
                break;
            case UserState.WaitingPhone:
                session.OrderData.Phone = text;
                session.State = UserState.WaitingAddress;
                await SendMessage(chatId, "🏠 Введите адрес доставки:");
                break;
            case UserState.WaitingAddress:
                session.OrderData.Address = text;
                await CompleteOrder(chatId);
                break;
        }
    }
    
    private async Task CompleteOrder(long chatId) {
        try {
            var session = _userSessions[chatId];
            
            var orderDto = new OrderCreateDto {
                Items = session.Cart.Select(c => new OrderItemDto {
                    ProductId = c.ProductId,
                    Name = c.Name,
                    Price = c.Price,
                    Size = c.Size
                }).ToList(),
                Customer = new CustomerDto {
                    Name = session.OrderData.Name,
                    Phone = session.OrderData.Phone,
                    Address = session.OrderData.Address
                },
                Source = "Telegram",
                TelegramUserId = chatId
            };
            
            var json = JsonSerializer.Serialize(orderDto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/orders", content);
            
            if (response.IsSuccessStatusCode) {
                var responseJson = await response.Content.ReadAsStringAsync();
                var order = JsonSerializer.Deserialize<OrderDto>(responseJson);
                
                var text = $"✅ **Заказ оформлен!**\n\n";
                text += $"🏷️ Номер: **{order?.OrderNumber}**\n";
                text += $"💰 Сумма: **{session.Cart.Sum(c => c.Price * c.Quantity):N0} ₽**\n\n";
                text += "📞 Мы свяжемся с вами в ближайшее время!\n";
                text += "🚚 Доставка: 1-3 рабочих дня";
                
                await SendMessage(chatId, text);
                
                session.Cart.Clear();
                session.State = UserState.None;
                session.OrderData = new OrderData();
            } else {
                await SendMessage(chatId, "❌ Ошибка оформления заказа");
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Ошибка создания заказа: {ex.Message}");
            await SendMessage(chatId, "❌ Ошибка оформления заказа");
        }
    }
    
    private async Task ShowOrders(long chatId) {
        try {
            var response = await _httpClient.GetStringAsync($"{_apiBaseUrl}/orders?telegramUserId={chatId}");
            var orders = JsonSerializer.Deserialize<List<OrderDto>>(response);
            
            if (orders?.Any() == true) {
                var text = "📦 **Ваши заказы:**\n\n";
                
                foreach (var order in orders.Take(5)) {
                    text += $"🏷️ {order.OrderNumber}\n";
                    text += $"📅 {order.CreatedDate:dd.MM.yyyy}\n";
                    text += $"💰 {order.Total:N0} ₽\n";
                    text += $"📊 {order.Status}\n\n";
                }
                
                await SendMessage(chatId, text);
            } else {
                await SendMessage(chatId, "📦 У вас пока нет заказов\n\nСделайте первую покупку! 🛍️");
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Ошибка получения заказов: {ex.Message}");
            await SendMessage(chatId, "❌ Ошибка загрузки заказов");
        }
    }
    
    private async Task ShowAbout(long chatId) {
        var text = "ℹ️ **О магазине StepLy**\n\n";
        text += "🏪 Мы специализируемся на продаже качественных кроссовок\n";
        text += "✅ Только оригинальная продукция\n";
        text += "🚚 Доставка по всей России\n";
        text += "💳 Различные способы оплаты\n";
        text += "🔄 Обмен и возврат в течение 14 дней\n\n";
        text += "📞 **Контакты:**\n";
        text += "☎️ +7 (800) 123-45-67\n";
        text += "📧 info@steply.ru\n";
        text += "🌐 steply.ru";
        
        await SendMessage(chatId, text);
    }
    
    private async Task ClearCart(long chatId) {
        _userSessions[chatId].Cart.Clear();
        await SendMessage(chatId, "🗑️ Корзина очищена");
    }
    
    private async Task ShowChatShopping(long chatId) {
        var text = "💬 **Покупки в чате**\n\n";
        text += "Используйте меню ниже для навигации по магазину";
        
        await SendMessage(chatId, text);
    }
    
    private async Task SendMessage(long chatId, string text) {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
        var payload = new { chat_id = chatId, text = text, parse_mode = "Markdown" };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        try {
            await _httpClient.PostAsync(url, content);
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Ошибка отправки сообщения: {ex.Message}");
        }
    }
    
    private async Task SendMessageWithKeyboard(long chatId, string text, object keyboard) {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
        var payload = new { chat_id = chatId, text = text, parse_mode = "Markdown", reply_markup = keyboard };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        try {
            await _httpClient.PostAsync(url, content);
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Ошибка отправки сообщения: {ex.Message}");
        }
    }
    
    private async Task SendMessageWithInlineKeyboard(long chatId, string text, object keyboard) {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
        var payload = new { chat_id = chatId, text = text, parse_mode = "Markdown", reply_markup = keyboard };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        try {
            await _httpClient.PostAsync(url, content);
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Ошибка отправки сообщения: {ex.Message}");
        }
    }
}