using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.Attributes;
using ShoeShop.MultiTenantAdmin.Data;
using ShoeShop.MultiTenantAdmin.Models;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin {

    [Authorize]
    [AdminAuth]
    public class OrdersModel : PageModel {
        public OrdersModel(IAdminRepository repository) {
            this.repository = repository;
        }

        private IAdminRepository repository;

        public int CurrentPage { get; private set; }
        public int ElementsPerPage { get; private set; }
        public int TotalElementsCount { get; private set; }
        public OrderSorting Sorting { get; private set; }
        public OrderStatusFilter Filter { get; private set; }
        public IEnumerable<Order>? Orders { get; private set; }

        public async Task OnGetAsync(OrderSorting sorting, OrderStatusFilter filter, int pageIndex = 1) {

            Sorting = sorting;
            Filter = filter;
            CurrentPage = pageIndex;
            ElementsPerPage = 20;
            Orders = await repository.GetOrders(filter, sorting, pageIndex - 1, 20);
            TotalElementsCount = await repository.OrderCount(filter);
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(Guid orderId, int status) {
            Order? order = await repository.GetOrder(orderId);
            if (order != null) {
                order.SetStatus((OrderStatus)status);
                await repository.UpdateOrder(order);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid orderId) {
            await repository.DeleteOrder(orderId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAllAsync() {
            await repository.DeleteAllOrders();
            return RedirectToPage();
        }
    }
}
