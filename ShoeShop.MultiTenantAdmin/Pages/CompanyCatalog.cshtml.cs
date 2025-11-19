using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoeShop.MultiTenantAdmin.Data;
using ShoeShop.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.Services;

namespace ShoeShop.MultiTenantAdmin.Pages {
    [Authorize]
    public class CompanyCatalogModel : PageModel {
        private readonly CompanyService _companyService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _context;

        public CompanyCatalogModel(CompanyService companyService, UserManager<ApplicationUser> userManager, ApplicationContext context) {
            _companyService = companyService;
            _userManager = userManager;
            _context = context;
        }

        public Company? Company { get; set; }
        public List<Product> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string companyName) {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return RedirectToPage("/Account/Login");
            }

            Company = await _companyService.GetCompanyByShortNameAsync(companyName);
            if (Company == null) {
                return NotFound();
            }

            var hasAccess = await _companyService.IsUserCompanyAdminAsync(user.Id, Company.Id);
            if (!hasAccess) {
                return Forbid();
            }

            // Загружаем товары (пока все, в будущем можно добавить фильтрацию по компании)
            Products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.DateAdded)
                .ToListAsync();

            return Page();
        }
    }
}