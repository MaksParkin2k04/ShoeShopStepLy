using Microsoft.AspNetCore.Mvc;
using ShoeShop.Models;

namespace ShoeShop.ViewComponents {
    public class CategoriesMenuViewComponent : ViewComponent {
        public CategoriesMenuViewComponent(IAdminRepository repository) {
            this.repository = repository;
        }

        private readonly IAdminRepository repository;

        public async Task<IViewComponentResult> InvokeAsync() {
            var categories = await repository.GetCategories();
            return View(categories);
        }
    }
}