using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Pages {
    public class ProductModel : PageModel {
        public ProductModel(IProductRepository repository, IBasketShoppingService basketShopping, StockService stockService, ReviewService reviewService, IProductStockRepository stockRepository) {
            this.repository = repository;
            this.basketShopping = basketShopping;
            this.stockService = stockService;
            this.reviewService = reviewService;
            this.stockRepository = stockRepository;
        }

        private IProductRepository repository;
        private readonly IBasketShoppingService basketShopping;
        private readonly StockService stockService;
        private readonly ReviewService reviewService;
        private readonly IProductStockRepository stockRepository;

        public Product? Product { get; private set; }
        public ProductAvailabilityStatus AvailabilityStatus { get; private set; }
        public Dictionary<int, int> SizeQuantities { get; private set; } = new();
        public List<ProductReview> Reviews { get; private set; } = new();
        public Dictionary<Guid, ReviewReply?> ReviewReplies { get; private set; } = new();
        public double AverageRating { get; private set; }
        public int ReviewCount { get; private set; }
        public bool CanReview { get; private set; }

        public async Task OnGetAsync(string id) {
            if (!Guid.TryParse(id, out Guid productId)) {
                return;
            }
            Product = await repository.GetProduct(productId);
            if (Product != null) {
                // Получаем остатки напрямую из репозитория
                var stocks = await stockRepository.GetByProductIdAsync(productId);
                SizeQuantities = stocks.ToDictionary(s => s.Size, s => s.Quantity);
                
                // Определяем статус наличия
                var totalStock = SizeQuantities.Values.Sum();
                if (totalStock == 0) {
                    AvailabilityStatus = ProductAvailabilityStatus.OutOfStock;
                } else if (totalStock < 5) {
                    AvailabilityStatus = ProductAvailabilityStatus.LowStock;
                } else {
                    AvailabilityStatus = ProductAvailabilityStatus.InStock;
                }
                Reviews = await reviewService.GetProductReviewsAsync(productId);
                AverageRating = await reviewService.GetAverageRatingAsync(productId);
                ReviewCount = await reviewService.GetReviewCountAsync(productId);
                
                // Загружаем ответы админа
                foreach (var review in Reviews) {
                    ReviewReplies[review.Id] = await reviewService.GetReviewReplyAsync(review.Id);
                }
                
                // Проверяем право на отзыв
                if (User.Identity.IsAuthenticated) {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId)) {
                        CanReview = await reviewService.CanUserReviewProductAsync(userId, productId);
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(Guid productId, int selectedSize) {
            // Проверяем наличие товара
            var quantity = await stockService.GetQuantityBySizeAsync(productId, selectedSize);
            if (quantity <= 0) {
                TempData["Error"] = "Товар недоступен в выбранном размере";
                return RedirectToPage("/Product", new { id = productId });
            }

            BasketShopping b = basketShopping.GetBasketShopping();
            b.Products.Add(new BasketItem { ProductId = productId, Size = selectedSize });
            basketShopping.SetBasketShopping(b);

            return RedirectToPage("/Product", new { id = productId });
        }

        public async Task<IActionResult> OnPostAddReviewAsync(Guid productId, int rating, string comment) {
            if (!User.Identity.IsAuthenticated) {
                TempData["Error"] = "Для добавления отзыва необходимо войти в систему";
                return RedirectToPage("/Product", new { id = productId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId)) {
                // Проверяем право на отзыв
                var canReview = await reviewService.CanUserReviewProductAsync(userId, productId);
                if (!canReview) {
                    TempData["Error"] = "Вы можете оставлять отзывы только на товары из выполненных заказов";
                    return RedirectToPage("/Product", new { id = productId });
                }
                
                await reviewService.AddReviewAsync(productId, userId, rating, comment ?? "");
            }
            
            return RedirectToPage("/Product", new { id = productId });
        }

        public async Task<IActionResult> OnPostAddReplyAsync(Guid productId, Guid reviewId, string reply) {
            if (!User.IsInRole("Admin")) {
                TempData["Error"] = "Недостаточно прав для ответа на отзыв";
                return RedirectToPage("/Product", new { id = productId });
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(adminId)) {
                await reviewService.AddAdminReplyAsync(reviewId, adminId, reply ?? "");
            }
            
            return RedirectToPage("/Product", new { id = productId });
        }
    }
}
