using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Services;

namespace ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Pages {
    [Authorize]
    public class CompanySelectModel : PageModel {
        private readonly CompanyService _companyService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanySelectModel(CompanyService companyService, UserManager<ApplicationUser> userManager) {
            _companyService = companyService;
            _userManager = userManager;
        }

        public List<Company> UserCompanies { get; set; } = new();

        public async Task OnGetAsync() {
            var user = await _userManager.GetUserAsync(User);
            if (user != null) {
                UserCompanies = await _companyService.GetUserCompaniesAsync(user.Id);
            }
        }
    }
}
