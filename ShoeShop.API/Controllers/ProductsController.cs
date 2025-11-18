using Microsoft.AspNetCore.Mvc;
using ShoeShop.Shared.DTOs;

namespace ShoeShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase {
    
    [HttpGet]
    public IActionResult GetProducts() {
        var products = new List<ProductDto> {
            new ProductDto {
                Id = Guid.NewGuid(),
                Name = "Nike Air Max",
                Content = "Удобные кроссовки для повседневной носки",
                Price = 8999,
                SalePrice = 7999,
                Category = "Кроссовки",
                Sizes = new List<int> { 40, 41, 42, 43 }
            },
            new ProductDto {
                Id = Guid.NewGuid(),
                Name = "Adidas Ultraboost",
                Content = "Профессиональные беговые кроссовки",
                Price = 12999,
                SalePrice = null,
                Category = "Кроссовки",
                Sizes = new List<int> { 39, 40, 41, 42, 43, 44 }
            },
            new ProductDto {
                Id = Guid.NewGuid(),
                Name = "Converse All Star",
                Content = "Классические кеды в стиле casual",
                Price = 4999,
                SalePrice = 3999,
                Category = "Кеды",
                Sizes = new List<int> { 38, 39, 40, 41, 42 }
            }
        };
        
        return Ok(products);
    }
    
    [HttpGet("{id}")]
    public IActionResult GetProduct(string id) {
        return NotFound();
    }
}