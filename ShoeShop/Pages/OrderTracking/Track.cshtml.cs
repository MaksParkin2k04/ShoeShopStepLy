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

        public async Task<IActionResult> OnGetAsync(string orderId)
        {
            Order = await repository.GetOrder(orderId);
            
            if (Order == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(string orderId, OrderStatus status)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            Order = await repository.GetOrder(orderId);
            if (Order != null)
            {
                Order.SetStatus(status);
                await repository.UpdateOrder(Order);
            }

            return RedirectToPage(new { orderId });
        }
    }
}