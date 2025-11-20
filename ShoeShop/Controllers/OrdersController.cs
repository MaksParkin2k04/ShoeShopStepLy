using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeShop.Models;
using System.Security.Claims;

namespace ShoeShop.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase {
        private readonly IProductRepository _repository;

        public OrdersController(IProductRepository repository) {
            _repository = repository;
        }

        /// <summary>
        /// Получить заказ по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(string id) {
            var order = await _repository.GetOrder(id);
            if (order == null) {
                return NotFound();
            }

            return Ok(order);
        }
    }
}