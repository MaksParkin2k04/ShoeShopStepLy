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

        public BasketShopping GetBasketShopping() {
            HttpRequest? request = httpContextAccessor.HttpContext?.Request;

            BasketShopping? basketShopping = null;

            if (request != null && request.Cookies.ContainsKey(Name)) {
                string? basket = request.Cookies[Name];
                if (basket != null) {
                    basketShopping = JsonSerializer.Deserialize<BasketShopping>(basket);
                }
            }

            if (basketShopping == null) {
                basketShopping = new BasketShopping();
                basketShopping.Products = new List<Guid>();
            }

            return basketShopping;
        }

        public void SetBasketShopping(BasketShopping basketShopping) {
            HttpResponse? response = httpContextAccessor.HttpContext?.Response;
            string json = JsonSerializer.Serialize(basketShopping);
            response?.Cookies.Append(Name, json);
        }

        public void Clear() {
            HttpResponse? response = httpContextAccessor.HttpContext?.Response;
            response?.Cookies.Delete(Name);
        }
    }
}
