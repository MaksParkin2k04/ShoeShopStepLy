using System.Text.Json;
using ShoeShop.Models;

namespace ShoeShop.Services {
    public class TelegramBotHandler {
        private readonly TelegramBotService _botService;
        private readonly IProductRepository _productRepository;
        private static readonly Dictionary<long, UserSession> _userSessions = new();
        
        public TelegramBotHandler(TelegramBotService botService, IProductRepository productRepository) {
            _botService = botService;
            _productRepository = productRepository;
        }
        
        public async Task HandleUpdateAsync(JsonElement update) {
            if (update.TryGetProperty("message", out var message)) {
                await HandleMessageAsync(message);
            } else if (update.TryGetProperty("callback_query", out var callbackQuery)) {
                await HandleCallbackQueryAsync(callbackQuery);
            }
        }
        
        private async Task HandleMessageAsync(JsonElement message) {
            var chatId = message.GetProperty("chat").GetProperty("id").GetInt64();
            var text = message.GetProperty("text").GetString() ?? "";
            
            var session = GetUserSession(chatId);
            
            switch (text) {
                case "/start":
                    await SendWelcomeMessage(chatId);
                    break;
                case "🛍️ Каталог":
                    await ShowCatalog(chatId);
                    break;
                case "🛒 Корзина":
                    await ShowCart(chatId);
                    break;
                case "📦 Мои заказы":
                    await ShowOrders(chatId);
                    break;
                default:
                    if (session.State == UserState.WaitingForName) {
                        session.OrderData.Name = text;
                        session.State = UserState.WaitingForPhone;
                        await _botService.SendMessageAsync(chatId, "📱 Введите ваш номер телефона:");
                    } else if (session.State == UserState.WaitingForPhone) {
                        session.OrderData.Phone = text;
                        session.State = UserState.WaitingForAddress;
                        await _botService.SendMessageAsync(chatId, "🏠 Введите адрес доставки:");
                    } else if (session.State == UserState.WaitingForAddress) {
                        session.OrderData.Address = text;
                        await ConfirmOrder(chatId);
                    }
                    break;
            }
        }
        
        private async Task HandleCallbackQueryAsync(JsonElement callbackQuery) {
            var chatId = callbackQuery.GetProperty("message").GetProperty("chat").GetProperty("id").GetInt64();
            var data = callbackQuery.GetProperty("data").GetString() ?? "";
            
            var parts = data.Split('_');
            var action = parts[0];
            
            switch (action) {
                case "category":
                    await ShowProductsByCategory(chatId, parts[1]);
                    break;
                case "product":
                    await ShowProduct(chatId, Guid.Parse(parts[1]));
                    break;
                case "add":
                    await AddToCart(chatId, Guid.Parse(parts[1]));
                    break;
                case "cart":
                    if (parts[1] == "remove") {
                        await RemoveFromCart(chatId, Guid.Parse(parts[2]));
                    } else if (parts[1] == "clear") {
                        await ClearCart(chatId);
                    } else if (parts[1] == "order") {
                        await StartOrder(chatId);
                    }
                    break;
            }
        }
        
        private async Task SendWelcomeMessage(long chatId) {
            var keyboard = new {
                keyboard = new[] {
                    new[] { new { text = "🛍️ Каталог" }, new { text = "🛒 Корзина" } },
                    new[] { new { text = "📦 Мои заказы" } }
                },
                resize_keyboard = true
            };
            
            await _botService.SendMessageWithKeyboardAsync(chatId, 
                "🛍️ Добро пожаловать в StepLy!\n\n" +
                "Выберите действие:", keyboard);
        }
        
        private async Task ShowCatalog(long chatId) {
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { 
                        new { text = "👨 Мужская", callback_data = "category_men" },
                        new { text = "👩 Женская", callback_data = "category_women" }
                    },
                    new[] { 
                        new { text = "👶 Детская", callback_data = "category_kids" },
                        new { text = "👀 Все товары", callback_data = "category_all" }
                    }
                }
            };
            
            await _botService.SendMessageWithInlineKeyboardAsync(chatId, 
                "📂 Выберите категорию:", keyboard);
        }
        
        private async Task ShowProductsByCategory(long chatId, string category) {
            var products = await _productRepository.GetAllAsync();
            var filteredProducts = category == "all" ? products : 
                products.Where(p => p.Category?.Name.ToLower().Contains(category) == true);
            
            if (!filteredProducts.Any()) {
                await _botService.SendMessageAsync(chatId, "😔 В этой категории пока нет товаров");
                return;
            }
            
            foreach (var product in filteredProducts.Take(10)) {
                var keyboard = new {
                    inline_keyboard = new[] {
                        new[] { new { text = "🛒 В корзину", callback_data = $"add_{product.Id}" } },
                        new[] { new { text = "📋 Подробнее", callback_data = $"product_{product.Id}" } }
                    }
                };
                
                var message = $"👟 *{product.Name}*\n\n" +
                             $"💰 Цена: *{product.FinalPrice:N0} ₽*\n" +
                             $"📝 {product.Description}";
                
                await _botService.SendMessageWithInlineKeyboardAsync(chatId, message, keyboard);
            }
        }
        
        private async Task ShowProduct(long chatId, Guid productId) {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) {
                await _botService.SendMessageAsync(chatId, "❌ Товар не найден");
                return;
            }
            
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { new { text = "🛒 Добавить в корзину", callback_data = $"add_{product.Id}" } }
                }
            };
            
            var message = $"👟 *{product.Name}*\n\n" +
                         $"💰 Цена: *{product.FinalPrice:N0} ₽*\n" +
                         $"📝 Описание: {product.Content}\n" +
                         $"📂 Категория: {product.Category?.Name}";
            
            await _botService.SendMessageWithInlineKeyboardAsync(chatId, message, keyboard);
        }
        
        private async Task AddToCart(long chatId, Guid productId) {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return;
            
            var session = GetUserSession(chatId);
            var existingItem = session.Cart.FirstOrDefault(i => i.ProductId == productId);
            
            if (existingItem != null) {
                existingItem.Quantity++;
            } else {
                session.Cart.Add(new CartItem {
                    ProductId = productId,
                    Name = product.Name,
                    Price = product.FinalPrice,
                    Quantity = 1
                });
            }
            
            await _botService.SendMessageAsync(chatId, $"✅ {product.Name} добавлен в корзину!");
        }
        
        private async Task ShowCart(long chatId) {
            var session = GetUserSession(chatId);
            
            if (!session.Cart.Any()) {
                await _botService.SendMessageAsync(chatId, "🛒 Ваша корзина пуста");
                return;
            }
            
            var message = "🛒 *Ваша корзина:*\n\n";
            var total = 0.0;
            
            foreach (var item in session.Cart) {
                message += $"• {item.Name}\n";
                message += $"  {item.Quantity} шт. × {item.Price:N0} ₽ = {item.Quantity * item.Price:N0} ₽\n\n";
                total += item.Quantity * item.Price;
            }
            
            message += $"💰 *Итого: {total:N0} ₽*";
            
            var keyboard = new {
                inline_keyboard = new[] {
                    new[] { new { text = "📦 Оформить заказ", callback_data = "cart_order" } },
                    new[] { new { text = "🗑️ Очистить корзину", callback_data = "cart_clear" } }
                }
            };
            
            await _botService.SendMessageWithInlineKeyboardAsync(chatId, message, keyboard);
        }
        
        private async Task StartOrder(long chatId) {
            var session = GetUserSession(chatId);
            if (!session.Cart.Any()) {
                await _botService.SendMessageAsync(chatId, "🛒 Корзина пуста");
                return;
            }
            
            session.State = UserState.WaitingForName;
            await _botService.SendMessageAsync(chatId, "👤 Введите ваше имя:");
        }
        
        private async Task ConfirmOrder(long chatId) {
            var session = GetUserSession(chatId);
            var total = session.Cart.Sum(i => i.Quantity * i.Price);
            
            var message = "📋 *Подтверждение заказа:*\n\n";
            message += $"👤 Имя: {session.OrderData.Name}\n";
            message += $"📱 Телефон: {session.OrderData.Phone}\n";
            message += $"🏠 Адрес: {session.OrderData.Address}\n\n";
            message += "🛒 *Товары:*\n";
            
            foreach (var item in session.Cart) {
                message += $"• {item.Name} - {item.Quantity} шт.\n";
            }
            
            message += $"\n💰 *Итого: {total:N0} ₽*";
            
            // Создаем заказ
            var orderId = Guid.NewGuid();
            
            // Отправляем уведомление
            await _botService.SendMessageAsync(chatId, 
                $"✅ Заказ #{orderId.ToString().Substring(0, 8)} успешно оформлен!\n\n" +
                "📞 Мы свяжемся с вами в ближайшее время для подтверждения.");
            
            // Очищаем сессию
            session.Cart.Clear();
            session.State = UserState.Default;
            session.OrderData = new OrderData();
        }
        
        private async Task RemoveFromCart(long chatId, Guid productId) {
            var session = GetUserSession(chatId);
            session.Cart.RemoveAll(i => i.ProductId == productId);
            await _botService.SendMessageAsync(chatId, "🗑️ Товар удален из корзины");
            await ShowCart(chatId);
        }
        
        private async Task ClearCart(long chatId) {
            var session = GetUserSession(chatId);
            session.Cart.Clear();
            await _botService.SendMessageAsync(chatId, "🗑️ Корзина очищена");
        }
        
        private async Task ShowOrders(long chatId) {
            await _botService.SendMessageAsync(chatId, "📦 У вас пока нет заказов");
        }
        
        private UserSession GetUserSession(long chatId) {
            if (!_userSessions.ContainsKey(chatId)) {
                _userSessions[chatId] = new UserSession();
            }
            return _userSessions[chatId];
        }
    }
    
    public class UserSession {
        public List<CartItem> Cart { get; set; } = new();
        public UserState State { get; set; } = UserState.Default;
        public OrderData OrderData { get; set; } = new();
    }
    
    public class CartItem {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
    
    public class OrderData {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
    }
    
    public enum UserState {
        Default,
        WaitingForName,
        WaitingForPhone,
        WaitingForAddress
    }
}