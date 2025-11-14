using Microsoft.AspNetCore.Mvc;
using ShoeShop.Models;

namespace ShoeShop.ViewComponents {
    public class CategoriesOptionsViewComponent : ViewComponent {
        public CategoriesOptionsViewComponent(IAdminRepository repository) {
            this.repository = repository;
        }

        private readonly IAdminRepository repository;

        public async Task<IViewComponentResult> InvokeAsync(Guid? selectedCategoryId = null) {
            var categories = await repository.GetCategories();
            ViewBag.SelectedCategoryId = selectedCategoryId;
            return View(categories);
        }
    }
}