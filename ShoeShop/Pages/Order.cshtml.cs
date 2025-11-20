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

        public async Task OnGetAsync(string orderId) {
            Order = await productRepository.GetOrder(orderId);
        }

        public async Task<IActionResult> OnPostCanselAsync(string orderId) {
            Order? order = await productRepository.GetOrder(orderId);
            order.SetStatus(OrderStatus.Canceled);
            await productRepository.UpdateOrder(order);
            return RedirectToPage("/Order", new { orderId = orderId });
        }
        
        public async Task<IActionResult> OnPostAddCustomerCommentAsync(string orderId, string comment) {
            if (!string.IsNullOrEmpty(comment)) {
                Order? order = await productRepository.GetOrder(orderId);
                if (order != null) {
                    order.AddCustomerComment(comment);
                    await productRepository.UpdateOrder(order);
                }
            }
            return RedirectToPage("/Order", new { orderId = orderId });
        }
    }
}
