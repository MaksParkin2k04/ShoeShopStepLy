using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;
using ShoeShop.Models;
using System.Text.Json;

namespace ShoeShop.Services {
    public class TelegramShopService : BackgroundService {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramShopService> _logger;
        private readonly string _botToken = "8468206640:AAFKsz7TklbKeaQbTIsmu__DzU01KK2sx1U";
        private long _lastUpdateId = 0;
        
        private static readonly Dictionary<long, TelegramUserSession> _userSessions = new();
        
        public TelegramShopService(IServiceProvider serviceProvider, ILogger<TelegramShopService> logger) {
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
                    _logger.LogError(ex, "Bot error");
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
            if (!_userSessions.ContainsKey(chatId)) {
                _userSessions[chatId] = new TelegramUserSession();
            }
            
            var session = _userSessions[chatId];
            
            if (session.State != TelegramUserState.None) {
                await HandleUserInput(chatId, text, httpClient);
                return;
            }
            
            switch (text) {
                case "/start":
                    await ShowWelcome(chatId, httpClient);
                    break;
                case "🛍️ Каталог":
                    await ShowCategories(chatId, httpClient);
                    break;
                case "🛒 Корзина":
                    await ShowCart(chatId, httpClient);
                    break;
                case "📦 Мои заказы":
                    await ShowOrders(chatId, httpClient);
                    break;
                case "🔍 Поиск":
                    await StartSearch(chatId, httpClient);
                    break;
                case "🎁 Акции":
                    await ShowPromotions(chatId, httpClient);
                    break;
                case "📞 Поддержка":
                    await ShowSupport(chatId, httpClient);
                    break;
                case "Начать покупки 🛒":
                    await ShowCategories(chatId, httpClient);
                    break;
                default:
                    if (session.State == TelegramUserState.Searching) {
                        await HandleSearch(chatId, text, httpClient);
                    } else {
                        await SendMessage(chatId, "Используйте меню для навигации", httpClient);
                    }
                    break;
            }
        }
        
        private async Task HandleCallback(JsonElement callback, HttpClient httpClient) {
            var chatId = callback.GetProperty("message").GetProperty("chat").GetProperty("id").GetInt64();
            var data = callback.GetProperty("data").GetString() ?? "";
            
            var parts = data.Split('_');
            if (parts.Length < 2) return;
            
            switch (parts[0]) {
                case "cat":
                    await ShowProducts(chatId, Guid.Parse(parts[1]), int.Parse(parts.Length > 2 ? parts[2] : "0"), httpClient);
                    break;
                case "prod":
                    await ShowProduct(chatId, Guid.Parse(parts[1]), httpClient);
                    break;
                case "add":
                    await AddToCart(chatId, Guid.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), httpClient);
                    break;
                case "cart":
                    if (parts[1] == "clear") await ClearCart(chatId, httpClient);
                    else if (parts[1] == "order") await StartOrder(chatId, httpClient);
                    break;
                case "order":
                    await ShowOrderDetail(chatId, Guid.Parse(parts[1]), httpClient);
                    break;
                case "menu":
                    await ShowCategories(chatId, httpClient);
                    break;
                case "start":
                    if (parts[1] == "shopping") await ShowCategories(chatId, httpClient);
                    break;
            }
        }
        
        private async Task ShowWelcome(long chatId, HttpClient httpClient) {
            var text = "👋 **Добро пожаловать в StepLy!**\n\n";
            text += "👟 Лучшие кроссовки от мировых брендов\n";
            text += "✨ Оригинальная продукция с гарантией\n";
            text += "🚚 Быстрая доставка по всей России";
            
            var keyboard = new {
                inline_keyboard = new object[][] {
                    new object[] { new { text = "🛍️ Открыть магазин", web_app = new { url = "https://jxpc5n7p-7002.euw.devtunnels.ms/telegram-app" } } },
                    new object[] { new { text = "Начать покупки 🛒", callback_data = "start_shopping" } }
                }
            };
            
            await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
            await SetMenuButton(chatId, httpClient);
        }
        
        private async Task SetMenuButton(long chatId, HttpClient httpClient) {
            var keyboard = new {
                keyboard = new[] {
                    new[] { new { text = "🛍️ Каталог" }, new { text = "🛒 Корзина" } },
                    new[] { new { text = "📦 Мои заказы" }, new { text = "🔍 Поиск" } },
                    new[] { new { text = "🎁 Акции" }, new { text = "📞 Поддержка" } }
                },
                resize_keyboard = true,
                persistent = true
            };
            
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new { 
                chat_id = chatId, 
                text = "📱 Меню активировано", 
                reply_markup = keyboard 
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, content);
        }
        
        private async Task ShowCategories(long chatId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            var categories = await context.Categories.ToListAsync();
            
            if (!categories.Any()) {
                await SendMessage(chatId, "Категории не найдены", httpClient);
                return;
            }
            
            var buttons = categories.Select(c => new[] { 
                new { text = c.Name, callback_data = $"cat_{c.Id}" } 
            }).ToList();
            
            buttons.Add(new[] { new { text = "🔙 Главное меню", callback_data = "menu" } });
            
            var keyboard = new { inline_keyboard = buttons.ToArray() };
            
            await SendMessageWithInlineKeyboard(chatId, "📂 **Категории товаров:**", keyboard, httpClient);
        }
        
        private async Task ShowProducts(long chatId, Guid categoryId, int page, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            var products = await context.Products
                .Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId && p.IsSale)
                .ToListAsync();
            
            if (!products.Any()) {
                await SendMessage(chatId, "Товары не найдены", httpClient);
                return;
            }
            
            const int pageSize = 5;
            var totalPages = (int)Math.Ceiling((double)products.Count / pageSize);
            var pageProducts = products.Skip(page * pageSize).Take(pageSize).ToList();
            
            var category = await context.Categories.FindAsync(categoryId);
            var text = $"👟 **{category?.Name}** (стр. {page + 1}/{totalPages})\n\n";
            
            foreach (var product in pageProducts) {
                text += $"🔸 **{product.Name}**\n";
                text += $"💰 {product.FinalPrice:N0} ₽\n";
                text += $"📝 {product.Description}\n\n";
            }
            
            var buttons = new List<object[]>();
            
            foreach (var product in pageProducts) {
                buttons.Add(new[] {
                    new { text = $"👀 {product.Name}", callback_data = $"prod_{product.Id}" }
                });
            }
            

            
            // Навигация
            var navButtons = new List<object>();
            if (page > 0) {
                navButtons.Add(new { text = "⬅️", callback_data = $"cat_{categoryId}_{page - 1}" });
            }
            navButtons.Add(new { text = $"📄 {page + 1}/{totalPages}", callback_data = "page_info" });
            if (page < totalPages - 1) {
                navButtons.Add(new { text = "➡️", callback_data = $"cat_{categoryId}_{page + 1}" });
            }
            if (navButtons.Count > 1) {
                buttons.Add(navButtons.ToArray());
            }
            
            buttons.Add(new[] { new { text = "🔙 Категории", callback_data = "menu" } });
            
            var productsKeyboard = new { inline_keyboard = buttons.ToArray() };
            
            await SendMessageWithInlineKeyboard(chatId, text, productsKeyboard, httpClient);
        }
        
        private async Task ShowProduct(long chatId, Guid productId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            var product = await context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);
            
            if (product == null) {
                await SendMessage(chatId, "Товар не найден", httpClient);
                return;
            }
            
            var text = $"👟 **{product.Name}**\n\n";
            text += $"💰 Цена: **{product.FinalPrice:N0} ₽**\n";
            text += $"📝 {product.Content}\n\n";
            
            if (!product.IsSale) {
                text += "❌ Нет в наличии";
                var unavailableKeyboard = new {
                    inline_keyboard = new[] {
                        new[] { new { text = "🔙 Назад", callback_data = $"cat_{product.CategoryId}" } }
                    }
                };
                
                if (product.Images.Any()) {
                    await SendPhoto(chatId, product.Images.First().Path, text, unavailableKeyboard, httpClient);
                } else {
                    await SendMessageWithInlineKeyboard(chatId, text, unavailableKeyboard, httpClient);
                }
                return;
            }
            
            text += "✅ В наличии\n\n👟 Выберите размер:";
            
            var sizes = GetAvailableSizes(product.Sizes);
            var buttons = new List<object[]>();
            
            foreach (var size in sizes) {
                buttons.Add(new[] {
                    new { text = $"Размер {size}", callback_data = $"add_{productId}_{size}_1" }
                });
            }
            
            buttons.Add(new[] { new { text = "🔙 Назад", callback_data = $"cat_{product.CategoryId}" } });
            
            var productSizesKeyboard = new { inline_keyboard = buttons.ToArray() };
            
            if (product.Images.Any()) {
                await SendPhoto(chatId, product.Images.First().Path, text, productSizesKeyboard, httpClient);
            } else {
                await SendMessageWithInlineKeyboard(chatId, text, productSizesKeyboard, httpClient);
            }
        }
        
        private async Task AddToCart(long chatId, Guid productId, int size, int quantity, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            var product = await context.Products.FindAsync(productId);
            if (product == null) return;
            
            if (!_userSessions.ContainsKey(chatId)) {
                _userSessions[chatId] = new TelegramUserSession();
            }
            
            var session = _userSessions[chatId];
            var existingItem = session.Cart.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
            
            if (existingItem != null) {
                existingItem.Quantity += quantity;
            } else {
                session.Cart.Add(new TelegramCartItem {
                    ProductId = productId,
                    Name = product.Name,
                    Price = product.FinalPrice,
                    Size = size,
                    Quantity = quantity
                });
            }
            
            await SendMessage(chatId, $"✅ {product.Name} (р.{size}) добавлен в корзину!", httpClient);
        }
        
        private async Task ShowCart(long chatId, HttpClient httpClient) {
            if (!_userSessions.ContainsKey(chatId) || !_userSessions[chatId].Cart.Any()) {
                await SendMessage(chatId, "🛒 Корзина пуста", httpClient);
                return;
            }
            
            var cart = _userSessions[chatId].Cart;
            var text = "🛒 **Ваша корзина:**\n\n";
            var total = 0.0;
            
            foreach (var item in cart) {
                text += $"• {item.Name} (р.{item.Size})\n";
                text += $"  {item.Quantity} шт. × {item.Price:N0} ₽ = {item.Price * item.Quantity:N0} ₽\n\n";
                total += item.Price * item.Quantity;
            }
            
            text += $"💰 **Итого: {total:N0} ₽**";
            
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { new { text = "📋 Оформить заказ", callback_data = "cart_order" } },
                    new[] { new { text = "🗑️ Очистить корзину", callback_data = "cart_clear" } },
                    new[] { new { text = "🔙 Главное меню", callback_data = "menu" } }
                }
            };
            
            await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
        }
        
        private async Task ClearCart(long chatId, HttpClient httpClient) {
            if (_userSessions.ContainsKey(chatId)) {
                _userSessions[chatId].Cart.Clear();
            }
            await SendMessage(chatId, "🗑️ Корзина очищена", httpClient);
        }
        
        private async Task StartOrder(long chatId, HttpClient httpClient) {
            if (!_userSessions.ContainsKey(chatId) || !_userSessions[chatId].Cart.Any()) {
                await SendMessage(chatId, "Корзина пуста", httpClient);
                return;
            }
            
            _userSessions[chatId].State = TelegramUserState.WaitingName;
            await SendMessage(chatId, "👤 Введите ваше имя:", httpClient);
        }
        
        private async Task HandleUserInput(long chatId, string text, HttpClient httpClient) {
            var session = _userSessions[chatId];
            
            switch (session.State) {
                case TelegramUserState.WaitingName:
                    session.OrderData.Name = text;
                    session.State = TelegramUserState.WaitingPhone;
                    await SendMessage(chatId, "📱 Введите номер телефона:", httpClient);
                    break;
                case TelegramUserState.WaitingPhone:
                    session.OrderData.Phone = text;
                    session.State = TelegramUserState.WaitingAddress;
                    await SendMessage(chatId, "🏠 Введите адрес доставки:", httpClient);
                    break;
                case TelegramUserState.WaitingAddress:
                    session.OrderData.Address = text;
                    await CompleteOrder(chatId, httpClient);
                    break;
            }
        }
        
        private async Task CompleteOrder(long chatId, HttpClient httpClient) {
            try {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                
                var session = _userSessions[chatId];
                var total = session.Cart.Sum(i => i.Price * i.Quantity);
                
                var recipient = OrderRecipient.Create(
                    session.OrderData.Name,
                    "Не указан",
                    session.OrderData.Address,
                    "", "",
                    session.OrderData.Phone
                );
                
                var orderDetails = new List<OrderDetail>();
                foreach (var item in session.Cart) {
                    for (int i = 0; i < item.Quantity; i++) {
                        orderDetails.Add(OrderDetail.Create(
                            item.ProductId,
                            "/images/no-image.jpg",
                            item.Name,
                            item.Price,
                            item.Size
                        ));
                    }
                }
                
                var order = Order.Create(
                    Guid.NewGuid(),
                    DateTime.Now,
                    $"Заказ из Telegram. Chat ID: {chatId}",
                    recipient,
                    orderDetails,
                    PaymentType.Cash
                );
                
                order.SetSource("Telegram");
                order.SetTelegramUser(chatId);
                var orderIdPart = order.Id.ToString().Length >= 6 ? order.Id.ToString().Substring(0, 6) : order.Id.ToString();
                order.SetOrderNumber($"TG{DateTime.Now:yyyyMMdd}{orderIdPart.ToUpper()}");
                
                context.Orders.Add(order);
                await context.SaveChangesAsync();
                
                var text = $"✅ **Заказ оформлен!**\n\n";
                text += $"🏷️ Номер: **{order.OrderNumber}**\n";
                text += $"📦 Товаров: {session.Cart.Sum(i => i.Quantity)} шт.\n";
                text += $"💰 Сумма: **{total:N0} ₽**\n\n";
                text += $"📞 Мы свяжемся с вами в ближайшее время!\n";
                text += $"🚚 Доставка: 1-3 рабочих дня";
                
                var keyboard = new {
                    inline_keyboard = new[] {
                        new[] { new { text = "🛍️ Продолжить покупки", callback_data = "menu" } }
                    }
                };
                
                await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
                
                session.Cart.Clear();
                session.State = TelegramUserState.None;
                session.OrderData = new TelegramOrderData();
                
            } catch (Exception ex) {
                _logger.LogError(ex, "Error completing order");
                await SendMessage(chatId, "❌ Ошибка оформления заказа", httpClient);
            }
        }
        
        private async Task ShowOrders(long chatId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            var orders = await context.Orders
                .Where(o => o.TelegramUserId == chatId)
                .OrderByDescending(o => o.CreatedDate)
                .Take(10)
                .ToListAsync();
            
            if (!orders.Any()) {
                var keyboard = new {
                    inline_keyboard = new[] {
                        new[] { new { text = "🛍️ Перейти к покупкам", callback_data = "menu" } }
                    }
                };
                await SendMessageWithInlineKeyboard(chatId, "📦 У вас пока нет заказов", keyboard, httpClient);
                return;
            }
            
            var text = "📦 **Ваши заказы:**\n\n";
            var buttons = new List<object[]>();
            
            foreach (var order in orders) {
                text += $"🏷️ {order.OrderNumber}\n";
                text += $"📅 {order.CreatedDate:dd.MM.yyyy}\n";
                text += $"📊 {GetStatusText(order.Status)}\n\n";
                
                buttons.Add(new[] {
                    new { text = $"📋 {order.OrderNumber}", callback_data = $"order_{order.Id}" }
                });
            }
            
            buttons.Add(new[] { new { text = "🔙 Главное меню", callback_data = "menu" } });
            
            var ordersListKeyboard = new { inline_keyboard = buttons.ToArray() };
            
            await SendMessageWithInlineKeyboard(chatId, text, ordersListKeyboard, httpClient);
        }
        
        private async Task ShowOrderDetail(long chatId, Guid orderId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            var order = await context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId.ToString() && o.TelegramUserId == chatId);
            
            if (order == null) {
                await SendMessage(chatId, "Заказ не найден", httpClient);
                return;
            }
            
            var text = $"📋 **Детали заказа**\n\n";
            text += $"🏷️ Номер: **{order.OrderNumber}**\n";
            text += $"📅 Дата: {order.CreatedDate:dd.MM.yyyy HH:mm}\n";
            text += $"📊 Статус: **{GetStatusText(order.Status)}**\n\n";
            
            if (order.OrderDetails?.Any() == true) {
                text += "🛍️ **Товары:**\n";
                var total = 0.0;
                
                foreach (var detail in order.OrderDetails) {
                    text += $"• {detail.Name} (р.{detail.Size})\n";
                    text += $"  {detail.Price:N0} ₽\n";
                    total += detail.Price;
                }
                
                text += $"\n💰 **Итого: {total:N0} ₽**";
            }
            
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { new { text = "📦 Мои заказы", callback_data = "menu" } },
                    new[] { new { text = "🔙 Главное меню", callback_data = "menu" } }
                }
            };
            
            await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
        }
        
        private async Task ShowProfile(long chatId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            var ordersCount = await context.Orders.CountAsync(o => o.TelegramUserId == chatId);
            
            var text = $"👤 **Ваш профиль**\n\n";
            text += $"🆔 ID: {chatId}\n";
            text += $"📦 Заказов: {ordersCount}\n";
            text += $"📅 Последняя активность: {DateTime.Now:dd.MM.yyyy HH:mm}";
            
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { new { text = "📦 Мои заказы", callback_data = "menu" } },
                    new[] { new { text = "🔙 Главное меню", callback_data = "menu" } }
                }
            };
            
            await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
        }
        
        private List<int> GetAvailableSizes(ProductSize sizes) {
            var availableSizes = new List<int>();
            
            for (int size = 35; size <= 46; size++) {
                var sizeFlag = (ProductSize)(1UL << (size - 1));
                if (sizes.HasFlag(sizeFlag)) {
                    availableSizes.Add(size);
                }
            }
            
            return availableSizes.Any() ? availableSizes : new List<int> { 40, 41, 42, 43 };
        }
        
        private string GetStatusText(OrderStatus status) {
            return status switch {
                OrderStatus.Created => "Создан",
                OrderStatus.Paid => "Оплачен",
                OrderStatus.Processing => "Обрабатывается",
                OrderStatus.Shipped => "Отправлен",
                OrderStatus.Completed => "Выполнен",
                OrderStatus.Canceled => "Отменен",
                _ => "Неизвестно"
            };
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
                photo = $"https://jxpc5n7p-7002.euw.devtunnels.ms{photoPath}",
                caption = caption,
                parse_mode = "Markdown",
                reply_markup = keyboard
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, content);
        }
        
        private async Task StartSearch(long chatId, HttpClient httpClient) {
            _userSessions[chatId].State = TelegramUserState.Searching;
            await SendMessage(chatId, "🔍 **Поиск товаров**\n\nВведите название или бренд кроссовок:", httpClient);
        }
        
        private async Task HandleSearch(long chatId, string query, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            var products = await context.Products
                .Include(p => p.Images)
                .Where(p => p.IsSale && (p.Name.Contains(query) || p.Description.Contains(query)))
                .Take(10)
                .ToListAsync();
            
            _userSessions[chatId].State = TelegramUserState.None;
            
            if (!products.Any()) {
                await SendMessage(chatId, $"😔 По запросу '{query}' ничего не найдено", httpClient);
                return;
            }
            
            var text = $"🔍 **Результаты поиска:** '{query}'\n\n";
            var buttons = new List<object[]>();
            
            foreach (var product in products) {
                text += $"👟 {product.Name} - {product.FinalPrice:N0} ₽\n";
                buttons.Add(new[] {
                    new { text = $"👀 {product.Name}", callback_data = $"prod_{product.Id}" }
                });
            }
            
            var searchKeyboard = new { inline_keyboard = buttons.ToArray() };
            await SendMessageWithInlineKeyboard(chatId, text, searchKeyboard, httpClient);
        }
        
        private async Task ShowPromotions(long chatId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            var saleProducts = await context.Products
                .Include(p => p.Images)
                .Where(p => p.IsSale && p.SalePrice.HasValue)
                .Take(5)
                .ToListAsync();
            
            var text = "🎁 **Акции и скидки**\n\n";
            var buttons = new List<object[]>();
            
            if (saleProducts.Any()) {
                foreach (var product in saleProducts) {
                    var discount = Math.Round((1 - (product.SalePrice.Value / product.Price)) * 100);
                    text += $"🔥 {product.Name}\n";
                    text += $"❌ ~~{product.Price:N0} ₽~~ → **{product.SalePrice:N0} ₽** (-{discount}%)\n\n";
                    
                    buttons.Add(new[] {
                        new { text = $"🛍️ {product.Name}", callback_data = $"prod_{product.Id}" }
                    });
                }
            } else {
                text += "😔 Акций пока нет";
            }
            
            var promoKeyboard = new { inline_keyboard = buttons.ToArray() };
            await SendMessageWithInlineKeyboard(chatId, text, promoKeyboard, httpClient);
        }
        
        private async Task ShowSupport(long chatId, HttpClient httpClient) {
            var text = "📞 **Поддержка клиентов**\n\n";
            text += "🕰 Рабочие часы: 9:00 - 21:00 (МСК)\n";
            text += "📞 Телефон: +7 (800) 123-45-67\n";
            text += "📧 Email: support@steply.ru\n\n";
            text += "💬 Либо опишите ваш вопрос - мы ответим в течение 15 минут!";
            
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { new { text = "📞 Позвонить", url = "tel:+78001234567" } },
                    new[] { new { text = "📧 Написать Email", url = "mailto:support@steply.ru" } }
                }
            };
            
            await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
        }
    }
    
    public class TelegramUserSession {
        public List<TelegramCartItem> Cart { get; set; } = new();
        public TelegramUserState State { get; set; } = TelegramUserState.None;
        public TelegramOrderData OrderData { get; set; } = new();
    }
    
    public class TelegramCartItem {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public int Size { get; set; }
        public int Quantity { get; set; }
    }
    
    public class TelegramOrderData {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
    }
    
    public enum TelegramUserState {
        None,
        WaitingName,
        WaitingPhone,
        WaitingAddress,
        Searching
    }
}