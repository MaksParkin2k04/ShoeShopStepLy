using System.Text.Json;
using ShoeShop.MultiTenantAdmin.Models;

namespace ShoeShop.MultiTenantAdmin.Services {
    public class TelegramPollingService : BackgroundService {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramPollingService> _logger;
        private readonly string _botToken = "8468206640:AAFKsz7TklbKeaQbTIsmu__DzU01KK2sx1U";
        private long _lastUpdateId = 0;
        private static readonly Dictionary<long, List<CartItem>> _userCarts = new();
        private static readonly Dictionary<long, OrderState> _userStates = new();
        private static readonly Dictionary<long, OrderData> _userOrders = new();
        
        public TelegramPollingService(IServiceProvider serviceProvider, ILogger<TelegramPollingService> logger) {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _logger.LogInformation("Telegram Polling Service started");
            
            while (!stoppingToken.IsCancellationRequested) {
                try {
                    await PollUpdates();
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Error in polling service");
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
                _logger.LogError(ex, "Error polling updates");
            }
        }
        
        private async Task HandleUpdate(JsonElement update, HttpClient httpClient) {
            try {
                if (update.TryGetProperty("callback_query", out var callbackQuery)) {
                    await HandleCallbackQuery(callbackQuery, httpClient);
                } else if (update.TryGetProperty("message", out var message)) {
                    var chatId = message.GetProperty("chat").GetProperty("id").GetInt64();
                    var text = message.GetProperty("text").GetString() ?? "";
                    
                    _logger.LogInformation($"Received message: {text} from {chatId}");
                    
                    switch (text) {
                        case "/start":
                            await SendWelcomeMessage(chatId, httpClient);
                            break;
                        case "🛍️ Каталог":
                            await SendCatalogMenu(chatId, httpClient);
                            break;
                        case "👨 Мужская":
                            await SendProductsByCategory(chatId, "Мужская", httpClient);
                            break;
                        case "👩 Женская":
                            await SendProductsByCategory(chatId, "Женская", httpClient);
                            break;
                        case "👶 Детская":
                            await SendProductsByCategory(chatId, "Детская", httpClient);
                            break;
                        case "🛒 Корзина":
                            await ShowCart(chatId, httpClient);
                            break;
                        case "📦 Заказы":
                            await SendMessage(chatId, "📦 У вас пока нет заказов", httpClient);
                            break;
                        default:
                            await HandleUserInput(chatId, text, httpClient);
                            break;
                    }
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error handling update");
            }
        }
        
        private async Task SendWelcomeMessage(long chatId, HttpClient httpClient) {
            var keyboard = new {
                keyboard = new[] {
                    new[] { new { text = "🛍️ Каталог" }, new { text = "🛒 Корзина" } },
                    new[] { new { text = "📦 Заказы" } }
                },
                resize_keyboard = true
            };
            
            await SendMessageWithKeyboard(chatId, 
                "🛍️ Добро пожаловать в StepLy!\n\n" +
                "Выберите действие из меню:", keyboard, httpClient);
        }
        
        private async Task SendCatalogMenu(long chatId, HttpClient httpClient) {
            var keyboard = new {
                keyboard = new[] {
                    new[] { new { text = "👨 Мужская" }, new { text = "👩 Женская" } },
                    new[] { new { text = "👶 Детская" } },
                    new[] { new { text = "🛍️ Каталог" }, new { text = "🛏️ Корзина" } }
                },
                resize_keyboard = true
            };
            
            await SendMessageWithKeyboard(chatId, 
                "📂 Выберите категорию:", keyboard, httpClient);
        }
        
        private async Task SendProductsByCategory(long chatId, string categoryName, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            
            try {
                var products = await productRepository.GetAllAsync();
                var categoryProducts = products.Where(p => p.IsSale && 
                    p.Category != null && p.Category.Name.Contains(categoryName)).Take(5);
                
                if (categoryProducts.Any()) {
                    await SendMessage(chatId, $"👟 *{categoryName} обувь:*", httpClient);
                    
                    foreach (var product in categoryProducts) {
                        var message = $"👟 *{product.Name}*\n" +
                                     $"💰 Цена: *{product.FinalPrice:N0} ₽*\n" +
                                     $"📝 {product.Description}";
                        
                        await SendMessage(chatId, message, httpClient);
                    }
                } else {
                    await SendMessage(chatId, $"😔 В категории '{categoryName}' пока нет товаров", httpClient);
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting products");
                await SendMessage(chatId, "❌ Ошибка загрузки каталога", httpClient);
            }
        }
        
        private async Task SendCatalogMessage(long chatId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            
            try {
                var products = await productRepository.GetAllAsync();
                var activeProducts = products.Where(p => p.IsSale).Take(5);
                
                if (activeProducts.Any()) {
                    await SendMessage(chatId, "📂 Наши товары:", httpClient);
                    
                    foreach (var product in activeProducts) {
                        var message = $"👟 *{product.Name}*\n" +
                                     $"💰 Цена: *{product.FinalPrice:N0} ₽*\n" +
                                     $"📝 {product.Description}";
                        
                        await SendMessage(chatId, message, httpClient);
                    }
                } else {
                    await SendMessage(chatId, "😔 Товары временно недоступны", httpClient);
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error getting products");
                await SendMessage(chatId, "❌ Ошибка загрузки каталога", httpClient);
            }
        }
        
        private async Task SendMessage(long chatId, string text, HttpClient httpClient) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new {
                chat_id = chatId,
                text = text,
                parse_mode = "Markdown"
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            await httpClient.PostAsync(url, content);
        }
        
        private async Task SendMessageWithKeyboard(long chatId, string text, object keyboard, HttpClient httpClient) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new {
                chat_id = chatId,
                text = text,
                parse_mode = "Markdown",
                reply_markup = keyboard
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            await httpClient.PostAsync(url, content);
        }
        
        private async Task HandleCallbackQuery(JsonElement callbackQuery, HttpClient httpClient) {
            var chatId = callbackQuery.GetProperty("message").GetProperty("chat").GetProperty("id").GetInt64();
            var data = callbackQuery.GetProperty("data").GetString() ?? "";
            
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
        
        private async Task SendProductWithButtons(long chatId, Product product, HttpClient httpClient) {
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { new { text = "🛒 В корзину", callback_data = $"add_{product.Id}" } }
                }
            };
            
            var message = $"👟 *{product.Name}*\n" +
                         $"💰 Цена: *{product.FinalPrice:N0} ₽*\n" +
                         $"📝 {product.Description}";
            
            // Отправляем фото если есть
            if (product.Images.Any()) {
                await SendPhotoWithKeyboard(chatId, product.Images.First().Path, message, keyboard, httpClient);
            } else {
                await SendMessageWithInlineKeyboard(chatId, message, keyboard, httpClient);
            }
        }
        
        private async Task SendPhotoWithKeyboard(long chatId, string photoPath, string caption, object keyboard, HttpClient httpClient) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendPhoto";
            var payload = new {
                chat_id = chatId,
                photo = $"https://yourdomain.com{photoPath}", // Замените на ваш домен
                caption = caption,
                parse_mode = "Markdown",
                reply_markup = keyboard
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            await httpClient.PostAsync(url, content);
        }
        
        private async Task SendMessageWithInlineKeyboard(long chatId, string text, object keyboard, HttpClient httpClient) {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new {
                chat_id = chatId,
                text = text,
                parse_mode = "Markdown",
                reply_markup = keyboard
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            await httpClient.PostAsync(url, content);
        }
        
        private async Task AddToCart(long chatId, Guid productId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            
            var product = await productRepository.GetByIdAsync(productId);
            if (product == null) return;
            
            if (!_userCarts.ContainsKey(chatId)) {
                _userCarts[chatId] = new List<CartItem>();
            }
            
            var existingItem = _userCarts[chatId].FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null) {
                existingItem.Quantity++;
            } else {
                _userCarts[chatId].Add(new CartItem {
                    ProductId = productId,
                    Name = product.Name,
                    Price = product.FinalPrice,
                    Quantity = 1
                });
            }
            
            await SendMessage(chatId, $"✅ {product.Name} добавлен в корзину!", httpClient);
        }
        
        private async Task ShowCart(long chatId, HttpClient httpClient) {
            if (!_userCarts.ContainsKey(chatId) || !_userCarts[chatId].Any()) {
                await SendMessage(chatId, "🛒 Ваша корзина пуста", httpClient);
                return;
            }
            
            var cart = _userCarts[chatId];
            var message = "🛒 *Ваша корзина:*\n\n";
            var total = 0.0;
            
            foreach (var item in cart) {
                message += $"• {item.Name}\n";
                message += $"  {item.Quantity} шт. × {item.Price:N0} ₽ = {item.Quantity * item.Price:N0} ₽\n\n";
                total += item.Quantity * item.Price;
            }
            
            message += $"💰 *Итого: {total:N0} ₽*";
            
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { new { text = "📋 Оформить заказ", callback_data = "order_start" } }
                }
            };
            
            await SendMessageWithInlineKeyboard(chatId, message, keyboard, httpClient);
        }
        
        private async Task StartOrder(long chatId, HttpClient httpClient) {
            if (!_userCarts.ContainsKey(chatId) || !_userCarts[chatId].Any()) {
                await SendMessage(chatId, "🛒 Корзина пуста", httpClient);
                return;
            }
            
            _userStates[chatId] = OrderState.WaitingForName;
            await SendMessage(chatId, "👤 Введите ваше имя:", httpClient);
        }
        
        private async Task HandleUserInput(long chatId, string text, HttpClient httpClient) {
            if (!_userStates.ContainsKey(chatId)) {
                await SendMessage(chatId, "Используйте кнопки меню для навигации", httpClient);
                return;
            }
            
            switch (_userStates[chatId]) {
                case OrderState.WaitingForName:
                    if (!_userOrders.ContainsKey(chatId)) {
                        _userOrders[chatId] = new OrderData();
                    }
                    _userOrders[chatId].Name = text;
                    _userStates[chatId] = OrderState.WaitingForPhone;
                    await SendMessage(chatId, "📱 Введите ваш номер телефона:", httpClient);
                    break;
                case OrderState.WaitingForPhone:
                    _userOrders[chatId].Phone = text;
                    _userStates[chatId] = OrderState.WaitingForAddress;
                    await SendMessage(chatId, "🏠 Введите адрес доставки:", httpClient);
                    break;
                case OrderState.WaitingForAddress:
                    _userOrders[chatId].Address = text;
                    await CompleteOrder(chatId, httpClient);
                    break;
            }
        }
        
        private async Task CompleteOrder(long chatId, HttpClient httpClient) {
            var cart = _userCarts[chatId];
            var orderData = _userOrders[chatId];
            var total = cart.Sum(i => i.Quantity * i.Price);
            var orderId = Guid.NewGuid();
            
            var message = $"✅ Ваш заказ #{orderId.ToString().Substring(0, 8)} успешно оформлен!\n\n" +
                         $"📦 Товаров: {cart.Sum(i => i.Quantity)}\n" +
                         $"💰 Сумма: {total:N0} ₽\n\n" +
                         $"📞 Мы свяжемся с вами в ближайшее время.";
            
            await SendMessage(chatId, message, httpClient);
            
            // Очищаем корзину и состояние
            _userCarts[chatId].Clear();
            _userStates.Remove(chatId);
            _userOrders.Remove(chatId);
        }
    }
    
    public enum OrderState {
        WaitingForName,
        WaitingForPhone,
        WaitingForAddress
    }
}
