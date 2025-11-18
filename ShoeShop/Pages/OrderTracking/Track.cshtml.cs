using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages.OrderTracking
{
    public class TrackModel : PageModel
    {
        private readonly IProductRepository repository;

        public TrackModel(IProductRepository repository)
        {
            this.repository = repository;
        }

        public ShoeShop.Models.Order? Order { get; set; }

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