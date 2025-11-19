using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.Services;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin {
    [Authorize]
    public class AdminIndexModel : PageModel {
        private readonly CompanyService _companyService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminIndexModel(CompanyService companyService, UserManager<ApplicationUser> userManager) {
            _companyService = companyService;
            _userManager = userManager;
        }

        public Company? Company { get; set; }

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

            return Page();
        }
        
        public IActionResult OnPostLogout() {
            return RedirectToPage("/companies");
        }
    }
}
