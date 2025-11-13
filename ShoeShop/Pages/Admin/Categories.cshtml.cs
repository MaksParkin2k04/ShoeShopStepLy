using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Pages.Admin {
    [Authorize(Roles = "Admin")]
    public class CategoriesModel : PageModel {
        public CategoriesModel(IAdminRepository repository) {
            this.repository = repository;
        }

        private IAdminRepository repository;

        public IEnumerable<Category>? Categories { get; private set; }

        public async Task OnGetAsync() {
            Categories = await repository.GetCategories();
        }

        public async Task<IActionResult> OnPostAddAsync(string categoryName) {
            if (!string.IsNullOrWhiteSpace(categoryName)) {
                Category category = Category.Create(categoryName.Trim());
                await repository.AddCategory(category);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid categoryId) {
            await repository.RemoveCategory(categoryId);
            return RedirectToPage();
        }
    }
}