using Microsoft.AspNetCore.Mvc;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase {
        private readonly IProductRepository _repository;
        private readonly StockService _stockService;
        private readonly ReviewService _reviewService;

        public ProductsController(IProductRepository repository, StockService stockService, ReviewService reviewService) {
            _repository = repository;
            _stockService = stockService;
            _reviewService = reviewService;
        }

        /// <summary>
        /// Получить список товаров
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
            [FromQuery] ProductSorting sorting = ProductSorting.Default,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) {
            
            var products = await _repository.GetProducts(sorting, (page - 1) * pageSize, pageSize);
            return Ok(products);
        }

        /// <summary>
        /// Получить товар по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id) {
            var product = await _repository.GetProduct(id);
            if (product == null) {
                return NotFound();
            }
            return Ok(product);
        }

        /// <summary>
        /// Получить остатки товара
        /// </summary>
        [HttpGet("{id}/stock")]
        public async Task<ActionResult> GetProductStock(Guid id) {
            var sizeQuantities = await _stockService.GetSizeQuantitiesAsync(id);
            var availability = await _stockService.GetAvailabilityStatusAsync(id);
            
            return Ok(new {
                ProductId = id,
                SizeQuantities = sizeQuantities,
                AvailabilityStatus = availability.ToString()
            });
        }

        /// <summary>
        /// Получить отзывы товара
        /// </summary>
        [HttpGet("{id}/reviews")]
        public async Task<ActionResult> GetProductReviews(Guid id) {
            var reviews = await _reviewService.GetProductReviewsAsync(id);
            var averageRating = await _reviewService.GetAverageRatingAsync(id);
            var reviewCount = await _reviewService.GetReviewCountAsync(id);
            
            return Ok(new {
                ProductId = id,
                Reviews = reviews,
                AverageRating = averageRating,
                ReviewCount = reviewCount
            });
        }
    }
}