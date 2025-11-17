using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages.Admin {
    [Authorize(Roles = "Admin")]
    public class OrderDetailModel : PageModel {
        public OrderDetailModel(IAdminRepository repository) {
            this.repository = repository;
        }

        private readonly IAdminRepository repository;

        public Order? Order { get; private set; }

        public async Task OnGetAsync(Guid orderId) {
            Order = await repository.GetOrder(orderId);
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(Guid orderId, int status) {
            Order? order = await repository.GetOrder(orderId);
            if (order != null) {
                order.SetStatus((OrderStatus)status);
                await repository.UpdateOrder(order);
            }
            return RedirectToPage("/Admin/OrderDetail", new { orderId = orderId });
        }
        
        public async Task<IActionResult> OnPostMarkAsPaidAsync(Guid orderId) {
            Order? order = await repository.GetOrder(orderId);
            if (order != null) {
                order.SetStatus(OrderStatus.Paid);
                await repository.UpdateOrder(order);
            }
            return RedirectToPage("/Admin/OrderDetail", new { orderId = orderId });
        }
        
        public async Task<IActionResult> OnPostAddCommentAsync(Guid orderId, string comment) {
            if (!string.IsNullOrEmpty(comment)) {
                Order? order = await repository.GetOrder(orderId);
                if (order != null) {
                    order.AddAdminComment(comment);
                    await repository.UpdateOrder(order);
                }
            }
            return RedirectToPage("/Admin/OrderDetail", new { orderId = orderId });
        }
    }
}