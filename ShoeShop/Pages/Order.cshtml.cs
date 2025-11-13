using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages {
    public class OrderModel : PageModel {
        public OrderModel(IProductRepository productRepository) {
            this.productRepository = productRepository;
        }

        private readonly IProductRepository productRepository;

        public Order? Order { get; private set; }

        public async Task OnGetAsync(Guid orderId) {
            Order = await productRepository.GetOrder(orderId);
        }

        public async Task<IActionResult> OnPostCanselAsync(Guid orderId) {
            Order? order = await productRepository.GetOrder(orderId);
            order.SetStatus(OrderStatus.Canceled);
            await productRepository.UpdateOrder(order);
            return RedirectToPage("/Order", new { orderId = orderId });
        }
    }
}
