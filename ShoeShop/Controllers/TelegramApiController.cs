using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;
using ShoeShop.Models;
using System.Text.Json;

namespace ShoeShop.Controllers {
    [ApiController]
    [Route("api/telegram")]
    public class TelegramApiController : ControllerBase {
        private readonly ApplicationContext _context;
        private readonly ILogger<TelegramApiController> _logger;
        
        public TelegramApiController(ApplicationContext context, ILogger<TelegramApiController> logger) {
            _context = context;
            _logger = logger;
        }
        
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts() {
            try {
                var products = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .Where(p => p.IsSale)
                    .Select(p => new {
                        id = p.Id,
                        name = p.Name,
                        description = p.Description,
                        content = p.Content,
                        price = p.Price,
                        salePrice = p.SalePrice,
                        image = p.Images.FirstOrDefault() != null ? p.Images.First().Path : null,
                        category = p.Category != null ? p.Category.Name.ToLower() : "",
                        dateAdded = p.DateAdded,
                        sizes = GetAvailableSizes(p.Sizes)
                    })
                    .ToListAsync();
                
                return Ok(products);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error getting products for Telegram");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
        
        [HttpPost("order")]
        public async Task<IActionResult> CreateOrder([FromBody] TelegramOrderRequest request) {
            try {
                if (request.Items == null || !request.Items.Any()) {
                    return BadRequest(new { error = "No items in order" });
                }
                
                var customerId = Guid.NewGuid();
                var userName = $"{request.User?.FirstName} {request.User?.LastName}".Trim();
                if (string.IsNullOrEmpty(userName)) {
                    userName = "Telegram User";
                }
                
                var recipient = OrderRecipient.Create(
                    userName,
                    "Не указан",
                    "Адрес будет уточнен",
                    "", "",
                    "Телефон будет уточнен"
                );
                
                var orderDetails = new List<OrderDetail>();
                foreach (var item in request.Items) {
                    for (int i = 0; i < item.Quantity; i++) {
                        orderDetails.Add(OrderDetail.Create(
                            item.Id,
                            item.Image ?? "/images/no-image.jpg",
                            item.Name,
                            item.Price,
                            item.Size
                        ));
                    }
                }
                
                var order = Order.Create(
                    customerId,
                    DateTime.Now,
                    "Заказ из Telegram Mini App",
                    recipient,
                    orderDetails,
                    PaymentType.Cash
                );
                
                order.SetSource("Telegram Mini App");
                if (request.User?.Id != null) {
                    order.SetTelegramUser(request.User.Id);
                }
                order.SetOrderNumber($"TMA{DateTime.Now:yyyyMMdd}{order.Id.ToString().Substring(0, 6).ToUpper()}");
                
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                
                return Ok(new {
                    success = true,
                    orderNumber = order.OrderNumber,
                    orderId = order.Id
                });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error creating Telegram order");
                return StatusCode(500, new { error = "Failed to create order" });
            }
        }
        
        [HttpGet("orders/{telegramUserId}")]
        public async Task<IActionResult> GetUserOrders(long telegramUserId) {
            try {
                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .Where(o => o.TelegramUserId == telegramUserId)
                    .OrderByDescending(o => o.CreatedDate)
                    .Select(o => new {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        createdDate = o.CreatedDate,
                        status = o.Status.ToString(),
                        total = o.OrderDetails != null ? o.OrderDetails.Sum(d => d.Price) : 0,
                        items = o.OrderDetails != null ? o.OrderDetails.Select(d => new {
                            name = d.Name,
                            price = d.Price,
                            size = d.Size,
                            image = d.Image
                        }).Cast<object>().ToList() : new List<object>()
                    })
                    .ToListAsync();
                
                return Ok(orders);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error getting user orders");
                return StatusCode(500, new { error = "Internal server error" });
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
    }
    
    public class TelegramOrderRequest {
        public List<TelegramOrderItem> Items { get; set; } = new();
        public double Total { get; set; }
        public TelegramUserInfo? User { get; set; }
    }
    
    public class TelegramOrderItem {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public int Size { get; set; }
        public int Quantity { get; set; }
        public string? Image { get; set; }
    }
    
    public class TelegramUserInfo {
        public long Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
    }
}