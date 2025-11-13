using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Pages {
    [Authorize(Roles = "Customer")]
    public class OrdersModel : PageModel {
        public OrdersModel(UserManager<ApplicationUser> userManager, IProductRepository repository) {
            this.userManager = userManager;
            this.repository = repository;
        }

        private UserManager<ApplicationUser> userManager;
        private IProductRepository repository;

        public int CurrentPage { get; private set; }
        public int ElementsPerPage { get; private set; }
        public int TotalElementsCount { get; private set; }
        public OrderSorting Sorting { get; private set; }
        public OrderStatusFilter Filter { get; private set; }
        public IEnumerable<Order>? Orders { get; private set; }

        public async Task OnGetAsync(OrderSorting sorting, OrderStatusFilter filter, int pageIndex = 1) {

            ApplicationUser? user = await userManager.GetUserAsync(User);

            Sorting = sorting;
            Filter = filter;
            CurrentPage = pageIndex;
            ElementsPerPage = 20;
            Orders = await repository.GetOrders(user.Id, filter, sorting, pageIndex - 1, 20);
            TotalElementsCount = await repository.OrderCount(user.Id, filter);
        }
    }
}
