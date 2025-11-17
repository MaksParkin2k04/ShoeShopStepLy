using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages
{
    [Authorize(Roles = "Admin")]
    public class OrderLabelModel : PageModel
    {
        private readonly IProductRepository repository;

        public OrderLabelModel(IProductRepository repository)
        {
            this.repository = repository;
        }

        public Order? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid orderId)
        {
            Order = await repository.GetOrder(orderId);
            
            if (Order == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}