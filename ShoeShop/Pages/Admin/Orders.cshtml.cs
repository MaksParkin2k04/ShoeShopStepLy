using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Attributes;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Pages.Admin {

    [Authorize]
    [AdminAuth("Admin", "Manager", "Consultant")]
    public class OrdersModel : PageModel {
        
        public bool CanEditOrders => User.IsInRole("Admin") || User.IsInRole("Manager");
        public bool CanDeleteOrders => User.IsInRole("Admin") || User.IsInRole("Manager");
        public OrdersModel(IAdminRepository repository) {
            this.repository = repository;
        }

        private IAdminRepository repository;

        public int CurrentPage { get; private set; }
        public int ElementsPerPage { get; private set; }
        public int TotalElementsCount { get; private set; }
        public OrderSorting Sorting { get; private set; }
        public OrderStatusFilter Filter { get; private set; }
        public string SearchQuery { get; private set; } = string.Empty;
        public IEnumerable<Order>? Orders { get; private set; }
        public Dictionary<OrderStatus, int> OrderStats { get; private set; } = new();

        public async Task OnGetAsync(OrderSorting sorting, OrderStatusFilter filter = OrderStatusFilter.Active, string search = "", int pageIndex = 1) {

            Sorting = sorting;
            Filter = filter;
            SearchQuery = search ?? string.Empty;
            CurrentPage = pageIndex;
            ElementsPerPage = 3;
            
            // Оптимизированная загрузка с кешированием
            Orders = await repository.GetOrdersFast(filter, sorting, pageIndex - 1, 3, SearchQuery);
            TotalElementsCount = await repository.OrderCountFast(filter, SearchQuery);
            OrderStats = await repository.GetOrderStatsCache();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(string orderId, int status) {
            Order? order = await repository.GetOrder(orderId);
            if (order != null) {
                order.SetStatus((OrderStatus)status);
                await repository.UpdateOrder(order);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string orderId) {
            await repository.DeleteOrder(orderId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAllAsync() {
            await repository.DeleteAllOrders();
            return RedirectToPage();
        }
    }
}
