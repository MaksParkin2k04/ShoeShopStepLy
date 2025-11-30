using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Data;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Pages {
    [Authorize]
    public class CreatingOrderModel : PageModel {
        public CreatingOrderModel(UserManager<ApplicationUser> userManager, IProductRepository repository, IBasketShoppingService basketShopping, StockService stockService, PromoCodeService promoCodeService) {
            this.userManager = userManager;
            this.repository = repository;
            this.basketShopping = basketShopping;
            this.stockService = stockService;
            this.promoCodeService = promoCodeService;
        }

        private UserManager<ApplicationUser> userManager;
        private IProductRepository repository;
        private readonly IBasketShoppingService basketShopping;
        private readonly StockService stockService;
        private readonly PromoCodeService promoCodeService;


        public IEnumerable<Product>? Products { get; private set; }
        public List<BasketProductInfo> BasketItems { get; private set; } = new List<BasketProductInfo>();
        
        // Данные пользователя для автозаполнения
        public string UserName { get; set; } = "";
        public string UserCity { get; set; } = "";
        public string UserStreet { get; set; } = "";
        public string UserHouse { get; set; } = "";
        public string UserApartment { get; set; } = "";
        public string UserPhone { get; set; } = "";
        
        [BindProperty]
        public string PromoCode { get; set; } = string.Empty;
        
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }

        public async Task OnGetAsync() {
            // Получаем данные пользователя для автозаполнения
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user != null) {
                UserName = $"{user.FirstName} {user.LastName}".Trim();
                UserCity = user.City ?? "";
                UserStreet = user.Street ?? "";
                UserHouse = user.House ?? "";
                UserApartment = user.Apartment ?? "";
                UserPhone = user.PhoneNumber ?? "";
            }
            
            BasketShopping bs = basketShopping.GetBasketShopping();
            Guid[] productIds = bs.Products.Select(p => p.ProductId).ToArray();
            Products = await repository.GetProducts(productIds);
            
            List<BasketProductInfo> list = new List<BasketProductInfo>();
            foreach (BasketItem item in bs.Products) {
                Product? product = Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null) {
                    list.Add(new BasketProductInfo { Product = product, Size = item.Size, Quantity = item.Quantity });
                }
            }
            BasketItems = list;
            
            // Вычисляем общую сумму заказа
            TotalAmount = (decimal)BasketItems.Sum(item => item.Product.Price * item.Quantity);
            
            // Проверяем, есть ли примененный промокод
            if (TempData["AppliedPromoCode"] != null && TempData["AppliedDiscount"] != null)
            {
                PromoCode = TempData["AppliedPromoCode"].ToString();
                DiscountAmount = decimal.Parse(TempData["AppliedDiscount"].ToString());
                FinalAmount = TotalAmount - DiscountAmount;
                
                // Сохраняем данные для следующего запроса
                TempData.Keep("AppliedPromoCode");
                TempData.Keep("AppliedDiscount");
            }
            else
            {
                FinalAmount = TotalAmount;
            }
        }

        public async Task<IActionResult> OnPostAsync(string name, string phone, string coment, int deliveryType, 
            // Курьерская доставка
            string city, string street, string house, string apartment,
            // Самовывоз
            string pickupPoint,
            // Почта России
            string postCity, string postIndex, string postAddress,
            // СДЭК
            string cdekCity, string cdekPoint,
            // Boxberry
            string boxberryCity, string boxberryPoint,
            Guid[] products) {

            BasketShopping bs = basketShopping.GetBasketShopping();
            if (bs.Products == null || bs.Products.Count == 0) {
                return RedirectToPage("/BasketShopping");
            }

            // Проверяем наличие товаров
            foreach (BasketItem basketItem in bs.Products) {
                bool isAvailable = await stockService.IsAvailableAsync(basketItem.ProductId, basketItem.Size, basketItem.Quantity);
                if (!isAvailable) {
                    TempData["Error"] = $"Недостаточно товара на складе. Проверьте корзину.";
                    return RedirectToPage("/BasketShopping");
                }
            }

            Guid[] productIds = bs.Products.Select(p => p.ProductId).ToArray();
            IEnumerable<Product> prod = await repository.GetProducts(productIds);

            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach (BasketItem basketItem in bs.Products) {
                Product? product = prod.FirstOrDefault(p => p.Id == basketItem.ProductId);
                if (product != null) {
                    string imagePath = product.Images?.Count > 0 ? product.Images[0].Path : "images/no-image.jpg";
                    for (int i = 0; i < basketItem.Quantity; i++) {
                        orderDetails.Add(OrderDetail.Create(product.Id, imagePath, product.Name ?? "", product.Price, basketItem.Size));
                    }
                }
            }

            if (orderDetails.Count == 0) {
                return RedirectToPage("/BasketShopping");
            }

            ApplicationUser? user = await userManager.GetUserAsync(User);
            
            // Применяем промокод если он был использован
            if (TempData["AppliedPromoCode"] != null)
            {
                string appliedPromoCode = TempData["AppliedPromoCode"].ToString();
                // Промокод был применен
            }

            // Формируем адрес в зависимости от способа доставки
            string finalCity = "", finalStreet = "", finalHouse = "", finalApartment = "";
            
            switch ((DeliveryType)deliveryType) {
                case DeliveryType.Courier:
                    finalCity = city ?? "";
                    finalStreet = street ?? "";
                    finalHouse = house ?? "";
                    finalApartment = apartment ?? "";
                    break;
                case DeliveryType.Pickup:
                    finalCity = "Пункт выдачи";
                    finalStreet = pickupPoint ?? "";
                    finalHouse = "";
                    finalApartment = "";
                    break;
                case DeliveryType.RussianPost:
                    finalCity = postCity ?? "";
                    finalStreet = postAddress ?? "";
                    finalHouse = postIndex ?? "";
                    finalApartment = "";
                    break;
                case DeliveryType.CDEK:
                    finalCity = cdekCity ?? "";
                    finalStreet = cdekPoint ?? "";
                    finalHouse = "";
                    finalApartment = "";
                    break;
                case DeliveryType.Boxberry:
                    finalCity = boxberryCity ?? "";
                    finalStreet = boxberryPoint ?? "";
                    finalHouse = "";
                    finalApartment = "";
                    break;
            }
            
            OrderRecipient recipient = OrderRecipient.Create(name ?? "", finalCity, finalStreet, finalHouse, finalApartment, phone ?? "");
            Order order = Order.Create(user!.Id, DateTime.Now, coment ?? "", recipient, orderDetails, PaymentType.Cash);
            order.SetSource("Сайт");
            order.SetWebUser(user.Id);
            order.GenerateOrderNumber();
            order.SetDeliveryType((DeliveryType)deliveryType);

            await repository.CreateOrder(order);
            
            // Уменьшаем остатки товаров
            foreach (BasketItem basketItem in bs.Products) {
                for (int i = 0; i < basketItem.Quantity; i++) {
                    await stockService.ReduceStockSafeAsync(basketItem.ProductId, basketItem.Size);
                }
            }
            
            basketShopping.Clear();

            return RedirectToPage("/Order", new { orderId = order.Id });
        }

        public async Task<IActionResult> OnPostCheckPromoCodeAsync(string promoCode)
        {
            if (!string.IsNullOrEmpty(promoCode))
            {
                var isValid = await promoCodeService.ValidatePromoCodeAsync(promoCode);
                if (isValid)
                {
                    BasketShopping bs = basketShopping.GetBasketShopping();
                    Guid[] productIds = bs.Products.Select(p => p.ProductId).ToArray();
                    var products = await repository.GetProducts(productIds);
                    
                    decimal totalAmount = 0;
                    foreach (BasketItem item in bs.Products)
                    {
                        var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                        if (product != null)
                        {
                            totalAmount += (decimal)(product.Price * item.Quantity);
                        }
                    }
                    
                    var discount = await promoCodeService.ApplyPromoCodeAsync(promoCode, totalAmount);
                    TempData["AppliedPromoCode"] = promoCode;
                    TempData["AppliedDiscount"] = discount.ToString();
                }
                else
                {
                    TempData["PromoError"] = "Неверный или неактивный промокод";
                }
            }
            
            return RedirectToPage();
        }
        
        public IActionResult OnPostRemovePromoCode()
        {
            TempData.Remove("AppliedPromoCode");
            TempData.Remove("AppliedDiscount");
            return RedirectToPage();
        }
    }
}
