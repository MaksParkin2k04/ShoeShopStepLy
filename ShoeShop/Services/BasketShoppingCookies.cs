using System.Text.Json;
using NuGet.ContentModel;
using ShoeShop.Models;

namespace ShoeShop.Services {
    public class BasketShoppingCookies : IBasketShoppingService {
        private const string Name = "basketshopping";

        public BasketShoppingCookies(IHttpContextAccessor httpContextAccessor) {
            this.httpContextAccessor = httpContextAccessor;
        }

        private readonly IHttpContextAccessor httpContextAccessor;

        private string GetCookieName() {
            var context = httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated == true) {
                // Для авторизованных пользователей используем ID пользователя
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                return $"{Name}_{userId}";
            } else {
                // Для анонимных пользователей используем ID сессии
                if (context?.Session != null) {
                    if (string.IsNullOrEmpty(context.Session.GetString("init"))) {
                        context.Session.SetString("init", "true");
                    }
                    return $"{Name}_{context.Session.Id}";
                }
                return $"{Name}_anonymous";
            }
        }

        public BasketShopping GetBasketShopping() {
            HttpRequest? request = httpContextAccessor.HttpContext?.Request;
            string cookieName = GetCookieName();

            BasketShopping? basketShopping = null;

            if (request != null && request.Cookies.ContainsKey(cookieName)) {
                string? basket = request.Cookies[cookieName];
                if (basket != null) {
                    try {
                        basketShopping = JsonSerializer.Deserialize<BasketShopping>(basket);
                    } catch {
                        // Очищаем старые cookies при ошибке десериализации
                        Clear();
                        basketShopping = null;
                    }
                }
            }

            if (basketShopping == null) {
                basketShopping = new BasketShopping();
                basketShopping.Products = new List<BasketItem>();
            }

            return basketShopping;
        }

        public void SetBasketShopping(BasketShopping basketShopping) {
            HttpResponse? response = httpContextAccessor.HttpContext?.Response;
            string cookieName = GetCookieName();
            string json = JsonSerializer.Serialize(basketShopping);
            response?.Cookies.Append(cookieName, json);
        }

        public void Clear() {
            HttpResponse? response = httpContextAccessor.HttpContext?.Response;
            string cookieName = GetCookieName();
            response?.Cookies.Delete(cookieName);
        }
    }
}
