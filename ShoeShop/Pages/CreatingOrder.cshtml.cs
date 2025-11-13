using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Pages {
    [Authorize]
    public class CreatingOrderModel : PageModel {
        public CreatingOrderModel(UserManager<ApplicationUser> userManager, IProductRepository repository, IBasketShoppingService basketShopping) {
            this.userManager = userManager;
            this.repository = repository;
            this.basketShopping = basketShopping;
        }

        private UserManager<ApplicationUser> userManager;
        private IProductRepository repository;
        private readonly IBasketShoppingService basketShopping;

        public IEnumerable<Product>? Products { get; private set; }

        public async Task OnGetAsync() {
            BasketShopping bs = basketShopping.GetBasketShopping();
            Products = await repository.GetProducts(bs.Products);
        }

        public async Task<IActionResult> OnPostAsync(string name, string city, string street, string house, string apartment, string phone, string coment, Guid[] products) {

            IEnumerable<Product> prod = await repository.GetProducts(products);

            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach (Product product in prod) {
                orderDetails.Add(OrderDetail.Create(product.Id, product.Images[0].Path, product.Name, product.Price));
            }

            ApplicationUser? user = await userManager.GetUserAsync(User);

            OrderRecipient recipient = OrderRecipient.Create(name, city, street, house, apartment, phone);
            Order order = Order.Create(user!.Id, DateTime.Now, coment, recipient, orderDetails);

            await repository.CreateOrder(order);
            basketShopping.Clear();

            return RedirectToPage("/Order", new { orderId = order.Id });
        }
    }
}
