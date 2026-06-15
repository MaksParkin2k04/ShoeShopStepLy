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

        public async Task<IActionResult> OnPostDeleteAsync(Guid categoryId, bool forceDelete = false) {
            // Проверяем, есть ли товары в категории
            var (hasProducts, productCount) = await repository.CheckCategoryHasProducts(categoryId);
            
            if (hasProducts && !forceDelete) {
                // Сохраняем информацию о категории для показа предупреждения
                TempData["CategoryToDelete"] = categoryId.ToString();
                TempData["ProductCount"] = productCount;
                return RedirectToPage();
            }
            
            if (hasProducts && forceDelete) {
                // Удаляем категорию вместе с товарами
                await repository.RemoveCategoryWithProducts(categoryId);
            } else {
                // Удаляем только категорию (товаров нет)
                await repository.RemoveCategory(categoryId);
            }
            
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetCheckProducts(Guid categoryId) {
            var (hasProducts, productCount) = await repository.CheckCategoryHasProducts(categoryId);
            return new JsonResult(new { hasProducts, productCount });
        }
    }
}