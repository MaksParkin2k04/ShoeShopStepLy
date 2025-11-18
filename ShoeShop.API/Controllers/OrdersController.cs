using Microsoft.AspNetCore.Mvc;
using ShoeShop.Shared.DTOs;

namespace ShoeShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase {
    
    [HttpGet]
    public IActionResult GetOrders([FromQuery] long? telegramUserId = null) {
        var orders = new List<OrderDto>();
        return Ok(orders);
    }
    
    [HttpGet("{id}")]
    public IActionResult GetOrder(string id) {
        return NotFound();
    }
    
    [HttpPost]
    public IActionResult CreateOrder(OrderCreateDto dto) {
        var orderNumber = "TMA" + DateTime.Now.ToString("yyyyMMddHHmmss");
        
        var order = new OrderDto {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CreatedDate = DateTime.Now,
            Status = "Новый",
            Total = dto.Items.Sum(i => i.Price),
            Source = "TelegramMiniApp",
            Items = dto.Items,
            Customer = dto.Customer
        };
        
        return Ok(order);
    }
    
    [HttpPost("user-data")]
    public IActionResult SaveUserData([FromBody] object userData) {
        return Ok(new { success = true });
    }
    
    [HttpPost("telegram-user")]
    public IActionResult SaveTelegramUser([FromBody] object telegramUserData) {
        return Ok(new { success = true });
    }
}