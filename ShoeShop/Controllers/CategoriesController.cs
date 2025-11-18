using Microsoft.AspNetCore.Mvc;
using ShoeShop.Models;

namespace ShoeShop.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase {
        private readonly IAdminRepository _repository;

        public CategoriesController(IAdminRepository repository) {
            _repository = repository;
        }

        /// <summary>
        /// Получить список категорий
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories() {
            var categories = await _repository.GetCategories();
            return Ok(categories);
        }
    }
}