using Microsoft.AspNetCore.Mvc;
using ShoeShop.Models;

namespace ShoeShop.ViewComponents {
    public class CategoriesFooterViewComponent : ViewComponent {
        public CategoriesFooterViewComponent(IAdminRepository repository) {
            this.repository = repository;
        }

        private readonly IAdminRepository repository;

        public async Task<IViewComponentResult> InvokeAsync() {
            var categories = await repository.GetCategories();
            return View(categories);
        }
    }
}