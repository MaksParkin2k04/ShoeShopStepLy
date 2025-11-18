using System.Text.Json;
using ShoeShop.Models;
using ShoeShop.Data;
using Microsoft.EntityFrameworkCore;

namespace ShoeShop.Services {
    public class TelegramBotPolling : BackgroundService {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramBotPolling> _logger;
        private readonly string _botToken = "8468206640:AAFKsz7TklbKeaQbTIsmu__DzU01KK2sx1U";
        private long _lastUpdateId = 0;
        
        // Хранилища данных пользователей
        private static readonly Dictionary<long, List<BotCartItem>> _carts = new();
        private static readonly Dictionary<long, BotOrderData> _orders = new();
        private static readonly Dictionary<long, string> _states = new();
        
        public TelegramBotPolling(IServiceProvider serviceProvider, ILogger<TelegramBotPolling> logger) {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                try {
                    await PollUpdates();
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Polling error");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
        
        private async Task PollUpdates() {
            using var scope = _serviceProvider.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
            
            var url = $"https://api.telegram.org/bot{_botToken}/getUpdates?offset={_lastUpdateId + 1}&timeout=30";
            
            try {
                var response = await httpClient.GetStringAsync(url);
                var json = JsonDocument.Parse(response);
                
                if (json.RootElement.GetProperty("ok").GetBoolean()) {
                    var updates = json.RootElement.GetProperty("result").EnumerateArray();
                    
                    foreach (var update in updates) {
                        _lastUpdateId = update.GetProperty("update_id").GetInt64();
                        await HandleUpdate(update, httpClient);
                    }
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error getting updates");
            }
        }
        
        private async Task HandleUpdate(JsonElement update, HttpClient httpClient) {
            try {
                if (update.TryGetProperty("callback_query", out var callback)) {
                    await HandleCallback(callback, httpClient);
                } else if (update.TryGetProperty("message", out var message)) {
                    var chatId = message.GetProperty("chat").GetProperty("id").GetInt64();
                    var text = message.GetProperty("text").GetString() ?? "";
                    
                    await HandleMessage(chatId, text, httpClient);
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error handling update");
            }
        }
        
        private async Task HandleMessage(long chatId, string text, HttpClient httpClient) {
            // Проверяем состояние пользователя
            if (_states.ContainsKey(chatId)) {
                await HandleOrderInput(chatId, text, httpClient);
                return;
            }
            
            switch (text) {
                case "/start":
                    await SendStart(chatId, httpClient);
                    break;
                case "🛍️ Каталог":
                    await SendCatalog(chatId, httpClient);
                    break;
                case "👨 Мужская":
                    await SendProducts(chatId, "Мужская", httpClient);
                    break;
                case "👩 Женская":
                    await SendProducts(chatId, "Женская", httpClient);
                    break;
                case "👶 Детская":
                    await SendProducts(chatId, "Детская", httpClient);
                    break;
                case "🛒 Корзина":
                    await SendCart(chatId, httpClient);
                    break;
                default:
                    await SendMessage(chatId, "Используйте кнопки меню", httpClient);
                    break;
            }
        }
        
        private async Task HandleCallback(JsonElement callback, HttpClient httpClient) {
            var chatId = callback.GetProperty("message").GetProperty("chat").GetProperty("id").GetInt64();
            var data = callback.GetProperty("data").GetString() ?? "";
            
            var parts = data.Split('_');
            if (parts.Length >= 2) {
                var action = parts[0];
                var productId = parts[1];
                
                switch (action) {
                    case "add":
                        await AddToCart(chatId, Guid.Parse(productId), httpClient);
                        break;
                    case "order":
                        await StartOrder(chatId, httpClient);
                        break;
                }
            }
        }
        
        private async Task SendStart(long chatId, HttpClient httpClient) {
            var keyboard = new {
                keyboard = new[] {
                    new[] { new { text = "🛍️ Каталог" }, new { text = "🛒 Корзина" } }
                },
                resize_keyboard = true
            };
            
            await SendMessageWithKeyboard(chatId, "🛍️ Добро пожаловать в StepLy!", keyboard, httpClient);
        }
        
        private async Task SendCatalog(long chatId, HttpClient httpClient) {
            var keyboard = new {
                keyboard = new[] {
                    new[] { new { text = "👨 Мужская" }, new { text = "👩 Женская" } },
                    new[] { new { text = "👶 Детская" } },
                    new[] { new { text = "🛍️ Каталог" }, new { text = "🛒 Корзина" } }
                },
                resize_keyboard = true
            };
            
            await SendMessageWithKeyboard(chatId, "📂 Выберите категорию:", keyboard, httpClient);
        }
        
        private async Task SendProducts(long chatId, string category, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            
            try {
                var products = await productRepo.GetAllAsync();
                var filtered = products.Where(p => p.IsSale && 
                    p.Category != null && p.Category.Name.Contains(category)).Take(5);
                
                if (filtered.Any()) {
                    await SendMessage(chatId, $"👟 {category} обувь:", httpClient);
                    
                    foreach (var product in filtered) {
                        var keyboard = new {
                            inline_keyboard = new[] {
                                new[] { new { text = "🛒 В корзину", callback_data = $"add_{product.Id}" } }
                            }
                        };
                        
                        var text = $"👟 *{product.Name}*\n💰 {product.FinalPrice:N0} ₽\n📝 {product.Description}";
                        
                        // Отправляем с фото если есть
                        if (product.Images.Any()) {
                            await SendPhoto(chatId, product.Images.First().Path, text, keyboard, httpClient);
                        } else {
                            await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
                        }
                    }
                } else {
                    await SendMessage(chatId, $"😔 В категории '{category}' пока нет товаров", httpClient);
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error loading products");
                await SendMessage(chatId, "❌ Ошибка загрузки товаров", httpClient);
            }
        }
        
        private async Task AddToCart(long chatId, Guid productId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            
            var product = await productRepo.GetByIdAsync(productId);
            if (product == null) return;
            
            if (!_carts.ContainsKey(chatId)) {
                _carts[chatId] = new List<BotCartItem>();
            }
            
            var existing = _carts[chatId].FirstOrDefault(i => i.ProductId == productId);
            if (existing != null) {
                existing.Quantity++;
            } else {
                _carts[chatId].Add(new BotCartItem {
                    ProductId = productId,
                    Name = product.Name,
                    Price = product.FinalPrice,
                    Quantity = 1
                });
            }
            
            await SendMessage(chatId, $"✅ {product.Name} добавлен в корзину!", httpClient);
        }
        
        private async Task SendCart(long chatId, HttpClient httpClient) {
            if (!_carts.ContainsKey(chatId) || !_carts[chatId].Any()) {
                await SendMessage(chatId, "🛒 Ваша корзина пуста", httpClient);
                return;
            }
            
            var cart = _carts[chatId];
            var text = "🛒 *Ваша корзина:*\n\n";
            var total = 0.0;
            
            foreach (var item in cart) {
                text += $"• {item.Name}\n  {item.Quantity} шт. × {item.Price:N0} ₽ = {item.Quantity * item.Price:N0} ₽\n\n";
                total += item.Quantity * item.Price;
            }
            
            text += $"💰 *Итого: {total:N0} ₽*";
            
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { new { text = "📋 Оформить заказ", callback_data = "order_start" } }
                }
            };
            
            await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
        }
        
        private async Task StartOrder(long chatId, HttpClient httpClient) {
            if (!_carts.ContainsKey(chatId) || !_carts[chatId].Any()) {
                await SendMessage(chatId, "🛒 Корзина пуста", httpClient);
                return;
            }
            
            _states[chatId] = "name";
            _orders[chatId] = new BotOrderData();
            await SendMessage(chatId, "👤 Введите ваше имя:", httpClient);
        }
        
        private async Task HandleOrderInput(long chatId, string text, HttpClient httpClient) {
            var state = _states[chatId];
            
            switch (state) {
                case "name":
                    _orders[chatId].Name = text;
                    _states[chatId] = "phone";
                    await SendMessage(chatId, "📱 Введите номер телефона:", httpClient);
                    break;
                case "phone":
                    _orders[chatId].Phone = text;
                    _states[chatId] = "address";
                    await SendMessage(chatId, "🏠 Введите адрес доставки:", httpClient);
                    break;
                case "address":
                    _orders[chatId].Address = text;
                    await CompleteOrder(chatId, httpClient);
                    break;
            }
        }
        
        private async Task CompleteOrder(long chatId, HttpClient httpClient) {
            var cart = _carts[chatId];
            var orderData = _orders[chatId];
            var total = cart.Sum(i => i.Quantity * i.Price);
            
            // Простое сохранение заказа
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            try {
                // Создаем простую запись в базе через SQL
                var orderId = Guid.NewGuid();
                var orderSql = $@"
                    INSERT INTO Orders (Id, CustomerId, CreatedDate, Status, PaymentType, Coment) 
                    VALUES ('{orderId}', '{Guid.NewGuid()}', '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 0, 0, 
                    'Заказ из Telegram. Chat ID: {chatId}. Клиент: {orderData.Name}, Телефон: {orderData.Phone}, Адрес: {orderData.Address}')";
                
                await context.Database.ExecuteSqlRawAsync(orderSql);
                
                var text = $"✅ Заказ #{orderId.ToString().Substring(0, 8)} оформлен!\n\n" +
                          $"📦 Товаров: {cart.Sum(i => i.Quantity)}\n" +
                          $"💰 Сумма: {total:N0} ₽\n\n" +
                          $"📞 Мы свяжемся с вами в ближайшее время.";
                
                await SendMessage(chatId, text, httpClient);
                _logger.LogInformation($"Order {orderId} created from Telegram chat {chatId}");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error saving order");
                await SendMessage(chatId, "❌ Ошибка оформления заказа", httpClient);
            }
            
            // Очищаем данные
            _carts[chatId].Clear();
            _states.Remove(chatId);
            _orders.Remove(chatId);
        }
        
        private async Task SendMessage(long chatId, string text, HttpClient httpClient) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new { chat_id = chatId, text = text, parse_mode = "Markdown" };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, content);
        }
        
        private async Task SendMessageWithKeyboard(long chatId, string text, object keyboard, HttpClient httpClient) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new { chat_id = chatId, text = text, parse_mode = "Markdown", reply_markup = keyboard };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, content);
        }
        
        private async Task SendMessageWithInlineKeyboard(long chatId, string text, object keyboard, HttpClient httpClient) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new { chat_id = chatId, text = text, parse_mode = "Markdown", reply_markup = keyboard };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, content);
        }
        
        private async Task SendPhoto(long chatId, string photoPath, string caption, object keyboard, HttpClient httpClient) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendPhoto";
            var payload = new {
                chat_id = chatId,
                photo = $"https://yourdomain.com{photoPath}",
                caption = caption,
                parse_mode = "Markdown",
                reply_markup = keyboard
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, content);
        }
    }
    
    public class BotCartItem {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
    
    public class BotOrderData {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
    }
}