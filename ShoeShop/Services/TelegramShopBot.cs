using System.Text.Json;
using ShoeShop.Models;
using ShoeShop.Data;
using Microsoft.EntityFrameworkCore;

namespace ShoeShop.Services {
    public class TelegramShopBot : BackgroundService {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramShopBot> _logger;
        private readonly string _botToken = "8468206640:AAFKsz7TklbKeaQbTIsmu__DzU01KK2sx1U";
        private long _lastUpdateId = 0;
        // Состояния пользователей
        private static readonly Dictionary<long, UserCart> _userCarts = new();
        private static readonly Dictionary<long, BotUserState> _userStates = new();
        private static readonly Dictionary<long, OrderInfo> _orderInfos = new();
        
        public TelegramShopBot(IServiceProvider serviceProvider, ILogger<TelegramShopBot> logger) {
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
                    _logger.LogError(ex, "Bot polling error");
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
            // Проверяем состояние оформления заказа
            if (_userStates.ContainsKey(chatId)) {
                var state = _userStates[chatId];
                if (state == BotUserState.SearchingOrder) {
                    await HandleOrderSearch(chatId, text, httpClient);
                } else if (state == BotUserState.WaitingEmail || state == BotUserState.WaitingPassword) {
                    await HandleAccountLinking(chatId, text, httpClient);
                } else {
                    await HandleOrderInput(chatId, text, httpClient);
                }
                return;
            }
            
            switch (text) {
                case "/start":
                    await SendMainMenu(chatId, httpClient);
                    break;
                case "🛍️ Каталог":
                    await SendCategories(chatId, httpClient);
                    break;
                case "🛒 Корзина":
                    await ShowCart(chatId, httpClient);
                    break;
                case "📦 Мои заказы":
                    await ShowUserOrders(chatId, httpClient);
                    break;
                case "🔍 Найти заказ":
                    await StartOrderSearch(chatId, httpClient);
                    break;
                case "👤 Профиль":
                    await ShowProfile(chatId, httpClient);
                    break;
                case "🔗 Связать аккаунт":
                    await StartAccountLinking(chatId, httpClient);
                    break;
                default:
                    await SendMessage(chatId, "Используйте кнопки меню для навигации", httpClient);
                    break;
            }
        }
        
        private async Task HandleCallback(JsonElement callback, HttpClient httpClient) {
            var chatId = callback.GetProperty("message").GetProperty("chat").GetProperty("id").GetInt64();
            var data = callback.GetProperty("data").GetString() ?? "";
            
            var parts = data.Split('_');
            if (parts.Length < 2) return;
            
            var action = parts[0];
            var param = parts[1];
            
            switch (action) {
                case "cat":
                    await ShowCategoryProducts(chatId, Guid.Parse(param), httpClient);
                    break;
                case "catpage":
                    if (parts.Length >= 3) {
                        await ShowCategoryProducts(chatId, Guid.Parse(param), httpClient, int.Parse(parts[2]));
                    }
                    break;
                case "prod":
                    await ShowProduct(chatId, Guid.Parse(param), httpClient);
                    break;
                case "addcart":
                    if (parts.Length >= 4) {
                        await AddToCart(chatId, Guid.Parse(param), int.Parse(parts[2]), httpClient, int.Parse(parts[3]));
                    }
                    break;
                case "size":
                    if (parts.Length >= 3) {
                        await ShowSizeSelection(chatId, Guid.Parse(param), int.Parse(parts[2]), httpClient);
                    }
                    break;
                case "cartplus":
                    await ChangeCartQuantity(chatId, Guid.Parse(param), 1, httpClient);
                    break;
                case "cartminus":
                    await ChangeCartQuantity(chatId, Guid.Parse(param), -1, httpClient);
                    break;
                case "cartdel":
                    await RemoveFromCart(chatId, Guid.Parse(param), httpClient);
                    break;
                case "order":
                    await StartOrder(chatId, httpClient);
                    break;
                case "profile":
                    await ShowProfile(chatId, httpClient);
                    break;
                case "myorders":
                    await ShowUserOrders(chatId, httpClient);
                    break;
                case "orderdetail":
                    if (parts.Length >= 2) {
                        await ShowOrderDetail(chatId, param, httpClient);
                    }
                    break;
                case "back":
                    if (param == "menu") await SendMainMenu(chatId, httpClient);
                    else if (param == "cat") await SendCategories(chatId, httpClient);
                    break;
                case "search":
                    if (param == "order") await StartOrderSearch(chatId, httpClient);
                    break;
                case "link":
                    if (param == "account") await StartAccountLinking(chatId, httpClient);
                    break;
            }
        }
        
        private async Task SendMainMenu(long chatId, HttpClient httpClient) {
            var keyboard = new {
                keyboard = new[] {
                    new[] { new { text = "🛍️ Каталог" }, new { text = "🛒 Корзина" } },
                    new[] { new { text = "📦 Мои заказы" }, new { text = "🔍 Найти заказ" } },
                    new[] { new { text = "👤 Профиль" } }
                },
                resize_keyboard = true
            };
            
            await SendMessageWithKeyboard(chatId, "🛍️ Добро пожаловать в StepLy!\n\nВыберите действие:", keyboard, httpClient);
        }
        
        private async Task SendCategories(long chatId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            try {
                var categories = await context.Categories.ToListAsync();
                
                if (!categories.Any()) {
                    await SendMessage(chatId, "😔 Категории пока не добавлены", httpClient);
                    return;
                }
                
                var buttons = categories.Select(c => new[] { 
                    new { text = c.Name, callback_data = $"cat_{c.Id}" } 
                }).ToArray();
                
                var keyboard = new {
                    inline_keyboard = buttons.Concat(new[] { 
                        new[] { new { text = "🔙 Главное меню", callback_data = "back_menu" } } 
                    }).ToArray()
                };
                
                await SendMessageWithInlineKeyboard(chatId, "📂 Выберите категорию:", keyboard, httpClient);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error loading categories");
                await SendMessage(chatId, "❌ Ошибка загрузки категорий", httpClient);
            }
        }
        
        private async Task ShowCategoryProducts(long chatId, Guid categoryId, HttpClient httpClient, int page = 0) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            try {
                var products = await context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == categoryId && p.IsSale)
                    .ToListAsync();
                
                if (!products.Any()) {
                    await SendMessage(chatId, "😔 В этой категории пока нет товаров", httpClient);
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
                
                // Кнопки товаров
                foreach (var product in pageProducts) {
                    buttons.Add(new[] {
                        new { text = $"👀 {product.Name}", callback_data = $"prod_{product.Id}" }
                    });
                }
                
                // Навигация
                var navButtons = new List<object>();
                if (page > 0) {
                    navButtons.Add(new { text = "⬅️ Назад", callback_data = $"catpage_{categoryId}_{page - 1}" });
                }
                if (page < totalPages - 1) {
                    navButtons.Add(new { text = "➡️ Далее", callback_data = $"catpage_{categoryId}_{page + 1}" });
                }
                if (navButtons.Any()) {
                    buttons.Add(navButtons.ToArray());
                }
                
                buttons.Add(new[] { new { text = "🔙 Категории", callback_data = "back_cat" } });
                
                var keyboard = new {
                    inline_keyboard = buttons.ToArray()
                };
                
                await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error loading products");
                await SendMessage(chatId, "❌ Ошибка загрузки товаров", httpClient);
            }
        }
        

        
        private async Task ShowProduct(long chatId, Guid productId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            try {
                var product = await context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == productId);
                
                if (product == null) {
                    await SendMessage(chatId, "❌ Товар не найден", httpClient);
                    return;
                }
                
                // Проверяем наличие
                if (!product.IsSale) {
                    var text = $"👟 *{product.Name}*\n\n" +
                              $"💰 Цена: *{product.FinalPrice:N0} ₽*\n" +
                              $"📝 {product.Content}\n\n" +
                              $"❌ *Товар нет в наличии*";
                    
                    var keyboard = new {
                        inline_keyboard = new[] {
                            new[] { new { text = "🔙 Назад", callback_data = $"cat_{product.CategoryId}" } }
                        }
                    };
                    
                    if (product.Images.Any()) {
                        await SendPhoto(chatId, product.Images.First().Path, text, keyboard, httpClient);
                    } else {
                        await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
                    }
                    return;
                }
                
                var productText = $"👟 *{product.Name}*\n\n" +
                                 $"💰 Цена: *{product.FinalPrice:N0} ₽*\n" +
                                 $"📝 {product.Content}\n\n" +
                                 $"✅ В наличии\n\n" +
                                 $"👟 Выберите размер:";
                
                var sizes = GetAvailableSizes(product.Sizes);
                var sizeButtons = new List<object[]>();
                
                // Разбиваем размеры по 4 в ряд
                for (int i = 0; i < sizes.Count; i += 4) {
                    var row = sizes.Skip(i).Take(4).Select(size => 
                        new { text = size.ToString(), callback_data = $"size_{productId}_{size}" }
                    ).ToArray();
                    sizeButtons.Add(row);
                }
                
                sizeButtons.Add(new[] { new { text = "🔙 Назад", callback_data = $"cat_{product.CategoryId}" } });
                
                var productKeyboard = new {
                    inline_keyboard = sizeButtons.ToArray()
                };
                
                if (product.Images.Any()) {
                    await SendPhoto(chatId, product.Images.First().Path, productText, productKeyboard, httpClient);
                } else {
                    await SendMessageWithInlineKeyboard(chatId, productText, productKeyboard, httpClient);
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error showing product");
                await SendMessage(chatId, "❌ Ошибка загрузки товара", httpClient);
            }
        }
        
        private async Task ShowSizeSelection(long chatId, Guid productId, int size, HttpClient httpClient) {
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { 
                        new { text = "1️⃣", callback_data = $"addcart_{productId}_{size}_1" },
                        new { text = "2️⃣", callback_data = $"addcart_{productId}_{size}_2" },
                        new { text = "3️⃣", callback_data = $"addcart_{productId}_{size}_3" }
                    },
                    new[] { 
                        new { text = "4️⃣", callback_data = $"addcart_{productId}_{size}_4" },
                        new { text = "5️⃣", callback_data = $"addcart_{productId}_{size}_5" }
                    },
                    new[] { new { text = "🔙 Назад", callback_data = $"prod_{productId}" } }
                }
            };
            
            await SendMessageWithInlineKeyboard(chatId, $"👟 Выберите количество\n📎 Размер: {size}", keyboard, httpClient);
        }
        
        private async Task AddToCart(long chatId, Guid productId, int size, HttpClient httpClient, int quantity = 1) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            try {
                var product = await context.Products.FindAsync(productId);
                if (product == null) return;
                
                if (!_userCarts.ContainsKey(chatId)) {
                    _userCarts[chatId] = new UserCart();
                }
                
                var cart = _userCarts[chatId];
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
                
                if (existingItem != null) {
                    existingItem.Quantity += quantity;
                } else {
                    cart.Items.Add(new CartItemBot {
                        ProductId = productId,
                        Name = product.Name,
                        Price = product.FinalPrice,
                        Size = size,
                        Quantity = quantity
                    });
                }
                
                await SendMessage(chatId, $"✅ {product.Name} (размер {size}) добавлен в корзину!", httpClient);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error adding to cart");
                await SendMessage(chatId, "❌ Ошибка добавления в корзину", httpClient);
            }
        }
        
        private async Task ShowCart(long chatId, HttpClient httpClient) {
            if (!_userCarts.ContainsKey(chatId) || !_userCarts[chatId].Items.Any()) {
                await SendMessage(chatId, "🛒 Ваша корзина пуста", httpClient);
                return;
            }
            
            var cart = _userCarts[chatId];
            var text = "🛒 *Ваша корзина:*\n\n";
            var total = 0.0;
            
            foreach (var item in cart.Items) {
                text += $"• {item.Name}\n";
                text += $"  Размер: {item.Size}, Количество: {item.Quantity}\n";
                text += $"  {item.Price:N0} ₽ × {item.Quantity} = {item.Price * item.Quantity:N0} ₽\n\n";
                total += item.Price * item.Quantity;
            }
            
            text += $"💰 *Итого: {total:N0} ₽*";
            
            var buttons = cart.Items.Select(item => new[] {
                new { text = $"➖ {item.Name} (р.{item.Size})", callback_data = $"cartminus_{item.ProductId}_{item.Size}" },
                new { text = $"➕", callback_data = $"cartplus_{item.ProductId}_{item.Size}" },
                new { text = "🗑️", callback_data = $"cartdel_{item.ProductId}_{item.Size}" }
            }).ToArray();
            
            var keyboard = new {
                inline_keyboard = buttons.Concat(new[] {
                    new[] { new { text = "📋 Оформить заказ", callback_data = "order_start" } },
                    new[] { new { text = "🔙 Главное меню", callback_data = "back_menu" } }
                }).ToArray()
            };
            
            await SendMessageWithInlineKeyboard(chatId, text, keyboard, httpClient);
        }
        
        private async Task ChangeCartQuantity(long chatId, Guid productId, int change, HttpClient httpClient) {
            if (!_userCarts.ContainsKey(chatId)) return;
            
            var cart = _userCarts[chatId];
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            
            if (item != null) {
                item.Quantity += change;
                if (item.Quantity <= 0) {
                    cart.Items.Remove(item);
                }
            }
            
            await ShowCart(chatId, httpClient);
        }
        
        private async Task RemoveFromCart(long chatId, Guid productId, HttpClient httpClient) {
            if (!_userCarts.ContainsKey(chatId)) return;
            
            var cart = _userCarts[chatId];
            cart.Items.RemoveAll(i => i.ProductId == productId);
            
            await ShowCart(chatId, httpClient);
        }
        
        private async Task StartOrder(long chatId, HttpClient httpClient) {
            if (!_userCarts.ContainsKey(chatId) || !_userCarts[chatId].Items.Any()) {
                await SendMessage(chatId, "🛒 Корзина пуста", httpClient);
                return;
            }
            
            _userStates[chatId] = BotUserState.WaitingName;
            _orderInfos[chatId] = new OrderInfo();
            
            await SendMessage(chatId, "👤 Введите ваше имя:", httpClient);
        }
        
        private async Task HandleOrderInput(long chatId, string text, HttpClient httpClient) {
            var state = _userStates[chatId];
            var orderInfo = _orderInfos[chatId];
            
            switch (state) {
                case BotUserState.WaitingName:
                    orderInfo.Name = text;
                    _userStates[chatId] = BotUserState.WaitingPhone;
                    await SendMessage(chatId, "📱 Введите номер телефона:", httpClient);
                    break;
                case BotUserState.WaitingPhone:
                    orderInfo.Phone = text;
                    _userStates[chatId] = BotUserState.WaitingAddress;
                    await SendMessage(chatId, "🏠 Введите адрес доставки:", httpClient);
                    break;
                case BotUserState.WaitingAddress:
                    orderInfo.Address = text;
                    await CompleteOrder(chatId, httpClient);
                    break;
            }
        }
        
        private async Task CompleteOrder(long chatId, HttpClient httpClient) {
            try {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                
                var cart = _userCarts[chatId];
                var orderInfo = _orderInfos[chatId];
                var total = cart.Items.Sum(i => i.Price * i.Quantity);
                var customerId = Guid.NewGuid();
                
                // Создаем или обновляем пользователя
                var user = await context.TelegramUsers.FirstOrDefaultAsync(u => u.TelegramId == chatId);
                if (user == null) {
                    user = new TelegramUser {
                        TelegramId = chatId,
                        FirstName = orderInfo.Name,
                        Phone = orderInfo.Phone,
                        Address = orderInfo.Address,
                        CreatedDate = DateTime.Now,
                        LastActivity = DateTime.Now
                    };
                    context.TelegramUsers.Add(user);
                } else {
                    user.Phone = orderInfo.Phone;
                    user.Address = orderInfo.Address;
                    user.LastActivity = DateTime.Now;
                }
                
                var recipient = OrderRecipient.Create(
                    orderInfo.Name,
                    "Не указан",
                    orderInfo.Address,
                    "",
                    "",
                    orderInfo.Phone
                );
                
                var orderDetails = new List<OrderDetail>();
                foreach (var item in cart.Items) {
                    for (int i = 0; i < item.Quantity; i++) {
                        var orderDetail = OrderDetail.Create(
                            item.ProductId,
                            "/images/no-image.jpg",
                            item.Name,
                            item.Price,
                            item.Size
                        );
                        orderDetails.Add(orderDetail);
                    }
                }
                
                var order = Order.Create(
                    customerId,
                    DateTime.Now,
                    $"Заказ из Telegram. Chat ID: {chatId}",
                    recipient,
                    orderDetails,
                    PaymentType.Cash
                );
                
                order.SetSource("Telegram");
                order.SetTelegramUser(chatId);
                order.SetOrderNumber($"TG{DateTime.Now:yyyyMMdd}{order.Id.ToString().Substring(0, 6).ToUpper()}");
                context.Orders.Add(order);
                await context.SaveChangesAsync();
                
                var text = $"✅ **Заказ оформлен!**\n\n" +
                          $"🏷️ Номер: **{order.OrderNumber}**\n" +
                          $"📦 Товаров: {cart.Items.Sum(i => i.Quantity)} шт.\n" +
                          $"💰 Сумма: **{total:N0} ₽**\n\n" +
                          $"📞 Мы свяжемся с вами в ближайшее время!\n\n" +
                          $"🚚 Доставка: 1-3 рабочих дня\n\n" +
                          $"💡 Сохраните номер заказа для отслеживания";
                
                var orderCompleteKeyboard = new {
                    inline_keyboard = new[] {
                        new[] { new { text = "🛍️ Продолжить покупки", callback_data = "back_menu" } }
                    }
                };
                
                await SendMessageWithInlineKeyboard(chatId, text, orderCompleteKeyboard, httpClient);
                
                _userCarts[chatId].Items.Clear();
                _userStates.Remove(chatId);
                _orderInfos.Remove(chatId);
                
                _logger.LogInformation($"Telegram order {order.Id} created successfully for chat {chatId}");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error completing order");
                await SendMessage(chatId, "❌ Ошибка оформления заказа. Попробуйте еще раз.", httpClient);
            }
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
        
        private async Task ShowProfile(long chatId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            var linkingService = scope.ServiceProvider.GetRequiredService<AccountLinkingService>();
            
            try {
                var user = await context.TelegramUsers.FirstOrDefaultAsync(u => u.TelegramId == chatId);
                if (user == null) {
                    user = new TelegramUser {
                        TelegramId = chatId,
                        FirstName = "Пользователь",
                        CreatedDate = DateTime.Now,
                        LastActivity = DateTime.Now
                    };
                    context.TelegramUsers.Add(user);
                    await context.SaveChangesAsync();
                }
                
                var allOrders = await linkingService.GetUnifiedOrdersAsync(chatId);
                var ordersCount = allOrders.Count;
                var isLinked = await linkingService.IsAccountLinkedAsync(chatId);
                
                var text = $"👤 **Ваш профиль**\n\n" +
                          $"👋 Имя: {user.GetFullName()}\n" +
                          $"📱 Телефон: {user.Phone ?? "Не указан"}\n" +
                          $"🏠 Адрес: {user.Address ?? "Не указан"}\n" +
                          $"📅 Регистрация: {user.CreatedDate:dd.MM.yyyy}\n" +
                          $"📦 Заказов: {ordersCount}\n" +
                          $"🔗 Связь с сайтом: {(isLinked ? "✅ Подключен" : "❌ Не подключен")}";
                
                var buttons = new List<object[]> {
                    new[] { new { text = "📦 Мои заказы", callback_data = "myorders" } }
                };
                
                if (!isLinked) {
                    buttons.Add(new[] { new { text = "🔗 Связать с сайтом", callback_data = "link_account" } });
                }
                
                buttons.Add(new[] { new { text = "🔙 Главное меню", callback_data = "back_menu" } });
                
                var profileKeyboard = new {
                    inline_keyboard = buttons.ToArray()
                };
                
                await SendMessageWithInlineKeyboard(chatId, text, profileKeyboard, httpClient);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error showing profile");
                await SendMessage(chatId, "❌ Ошибка загрузки профиля", httpClient);
            }
        }
        
        private async Task ShowUserOrders(long chatId, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var linkingService = scope.ServiceProvider.GetRequiredService<AccountLinkingService>();
            
            try {
                var orders = await linkingService.GetUnifiedOrdersAsync(chatId);
                orders = orders.Take(10).ToList();
                
                if (!orders.Any()) {
                    var noOrdersKeyboard = new {
                        inline_keyboard = new[] {
                            new[] { new { text = "🛍️ Перейти к покупкам", callback_data = "back_menu" } }
                        }
                    };
                    
                    await SendMessageWithInlineKeyboard(chatId, "📦 У вас пока нет заказов\n\nОформите первый заказ!", noOrdersKeyboard, httpClient);
                    return;
                }
                
                var text = "📦 *Ваши заказы:*\n\n";
                var buttons = new List<object[]>();
                
                foreach (var order in orders) {
                    var statusEmoji = GetStatusEmoji(order.Status);
                    text += $"{statusEmoji} {order.OrderNumber}\n";
                    text += $"📅 {order.CreatedDate:dd.MM.yyyy HH:mm}\n";
                    text += $"📊 {GetStatusText(order.Status)}\n\n";
                    
                    buttons.Add(new[] {
                        new { text = $"📋 {order.OrderNumber}", callback_data = $"orderdetail_{order.Id}" }
                    });
                }
                
                buttons.Add(new[] { new { text = "🔙 Главное меню", callback_data = "back_menu" } });
                
                var ordersListKeyboard = new {
                    inline_keyboard = buttons.ToArray()
                };
                
                await SendMessageWithInlineKeyboard(chatId, text, ordersListKeyboard, httpClient);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error showing user orders");
                await SendMessage(chatId, "❌ Ошибка загрузки заказов", httpClient);
            }
        }
        
        private async Task ShowOrderDetail(long chatId, string orderIdStr, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            try {
                if (!Guid.TryParse(orderIdStr, out var orderId)) return;
                
                var order = await context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == orderId.ToString() && o.TelegramUserId == chatId);
                
                if (order == null) {
                    await SendMessage(chatId, "❌ Заказ не найден", httpClient);
                    return;
                }
                
                var statusEmoji = GetStatusEmoji(order.Status);
                var text = $"📋 *Детали заказа*\n\n" +
                          $"🏷️ Номер: *{order.OrderNumber}*\n" +
                          $"📅 Дата: {order.CreatedDate:dd.MM.yyyy HH:mm}\n" +
                          $"{statusEmoji} Статус: *{GetStatusText(order.Status)}*\n\n";
                
                if (order.OrderDetails?.Any() == true) {
                    text += "🛍️ *Товары:*\n";
                    var total = 0.0;
                    
                    foreach (var detail in order.OrderDetails) {
                        text += $"• {detail.Name} (р.{detail.Size})\n";
                        text += $"  {detail.Price:N0} ₽\n";
                        total += detail.Price;
                    }
                    
                    text += $"\n💰 *Итого: {total:N0} ₽*\n\n";
                }
                
                if (order.Recipient != null) {
                    text += $"📞 Получатель: {order.Recipient.Name}\n";
                    text += $"📱 Телефон: {order.Recipient.Phone}\n";
                    text += $"🏠 Адрес: {order.Recipient.Street}";
                }
                
                var orderDetailKeyboard = new {
                    inline_keyboard = new[] {
                        new[] { new { text = "📦 Мои заказы", callback_data = "myorders" } },
                        new[] { new { text = "🔙 Главное меню", callback_data = "back_menu" } }
                    }
                };
                
                await SendMessageWithInlineKeyboard(chatId, text, orderDetailKeyboard, httpClient);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error showing order detail");
                await SendMessage(chatId, "❌ Ошибка загрузки заказа", httpClient);
            }
        }
        
        private async Task StartOrderSearch(long chatId, HttpClient httpClient) {
            _userStates[chatId] = BotUserState.SearchingOrder;
            await SendMessage(chatId, "🔍 Введите номер заказа для поиска:\n\nНапример: TG20241201ABC123", httpClient);
        }
        
        private async Task HandleOrderSearch(long chatId, string orderNumber, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            try {
                var order = await context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber.Trim());
                
                _userStates.Remove(chatId);
                
                if (order == null) {
                    await SendMessage(chatId, "❌ Заказ с таким номером не найден\n\nПроверьте правильность номера", httpClient);
                    return;
                }
                
                var statusEmoji = GetStatusEmoji(order.Status);
                var text = $"🔍 *Найден заказ*\n\n" +
                          $"🏷️ Номер: *{order.OrderNumber}*\n" +
                          $"📅 Дата: {order.CreatedDate:dd.MM.yyyy HH:mm}\n" +
                          $"{statusEmoji} Статус: *{GetStatusText(order.Status)}*\n\n";
                
                if (order.OrderDetails?.Any() == true) {
                    text += "🛍️ *Товары:*\n";
                    var total = 0.0;
                    
                    foreach (var detail in order.OrderDetails) {
                        text += $"• {detail.Name} (р.{detail.Size})\n";
                        text += $"  {detail.Price:N0} ₽\n";
                        total += detail.Price;
                    }
                    
                    text += $"\n💰 *Итого: {total:N0} ₽*";
                }
                
                var searchResultKeyboard = new {
                    inline_keyboard = new[] {
                        new[] { new { text = "🔍 Найти другой заказ", callback_data = "search_order" } },
                        new[] { new { text = "🔙 Главное меню", callback_data = "back_menu" } }
                    }
                };
                
                await SendMessageWithInlineKeyboard(chatId, text, searchResultKeyboard, httpClient);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error searching order");
                await SendMessage(chatId, "❌ Ошибка поиска заказа", httpClient);
                _userStates.Remove(chatId);
            }
        }
        
        private string GetStatusEmoji(OrderStatus status) {
            return status switch {
                OrderStatus.Created => "🆕",
                OrderStatus.Paid => "💳",
                OrderStatus.Processing => "🔄",
                OrderStatus.AwaitingShipment => "📦",
                OrderStatus.Shipped => "🚚",
                OrderStatus.InTransit => "🚛",
                OrderStatus.Arrived => "🏢",
                OrderStatus.ReadyForPickup => "✅",
                OrderStatus.Completed => "✅",
                OrderStatus.Returned => "↩️",
                OrderStatus.Canceled => "❌",
                _ => "❓"
            };
        }
        
        private string GetStatusText(OrderStatus status) {
            return status switch {
                OrderStatus.Created => "Создан",
                OrderStatus.Paid => "Оплачен",
                OrderStatus.Processing => "Обрабатывается",
                OrderStatus.AwaitingShipment => "Ожидает отправки",
                OrderStatus.Shipped => "Отправлен",
                OrderStatus.InTransit => "В пути",
                OrderStatus.Arrived => "Прибыл",
                OrderStatus.ReadyForPickup => "Готов к выдаче",
                OrderStatus.Completed => "Выполнен",
                OrderStatus.Returned => "Возвращен",
                OrderStatus.Canceled => "Отменен",
                _ => "Неизвестно"
            };
        }
        
        private async Task StartAccountLinking(long chatId, HttpClient httpClient) {
            _userStates[chatId] = BotUserState.WaitingEmail;
            _orderInfos[chatId] = new OrderInfo();
            
            await SendMessage(chatId, "🔗 **Связывание аккаунтов**\n\n📧 Введите email от аккаунта на сайте:", httpClient);
        }
        
        private async Task HandleAccountLinking(long chatId, string text, HttpClient httpClient) {
            using var scope = _serviceProvider.CreateScope();
            var linkingService = scope.ServiceProvider.GetRequiredService<AccountLinkingService>();
            
            var state = _userStates[chatId];
            var orderInfo = _orderInfos[chatId];
            
            switch (state) {
                case BotUserState.WaitingEmail:
                    orderInfo.Name = text; // Используем Name для email
                    _userStates[chatId] = BotUserState.WaitingPassword;
                    await SendMessage(chatId, "🔐 Введите пароль от аккаунта:", httpClient);
                    break;
                case BotUserState.WaitingPassword:
                    var email = orderInfo.Name;
                    var password = text;
                    
                    var success = await linkingService.LinkAccountsAsync(chatId, email, password);
                    
                    _userStates.Remove(chatId);
                    _orderInfos.Remove(chatId);
                    
                    if (success) {
                        await SendMessage(chatId, "✅ **Аккаунты успешно связаны!**\n\nТеперь вы можете видеть все свои заказы из Telegram и с сайта.", httpClient);
                    } else {
                        await SendMessage(chatId, "❌ **Ошибка связывания**\n\nПроверьте правильность email и пароля.", httpClient);
                    }
                    break;
            }
        }
    }
    
    public class UserCart {
        public List<CartItemBot> Items { get; set; } = new();
    }
    
    public class CartItemBot {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public int Size { get; set; }
        public int Quantity { get; set; }
    }
    
    public class OrderInfo {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
    }
    
    public enum BotUserState {
        WaitingName,
        WaitingPhone,
        WaitingAddress,
        SearchingOrder,
        WaitingEmail,
        WaitingPassword
    }
}