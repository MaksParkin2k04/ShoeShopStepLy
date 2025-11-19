using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.Services;

namespace ShoeShop.MultiTenantAdmin.Pages {
    [Authorize]
    public class CompanyProductsModel : PageModel {
        private readonly CompanyService _companyService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;

        public CompanyProductsModel(CompanyService companyService, UserManager<ApplicationUser> userManager, IProductRepository productRepository) {
            _companyService = companyService;
            _userManager = userManager;
            _productRepository = productRepository;
        }

        public Company? Company { get; set; }
        public IReadOnlyList<Product>? Products { get; set; }
        public int CurrentPage { get; private set; }
        public int TotalElementsCount { get; private set; }
        public ProductSorting Sorting { get; private set; }
        public IsSaleFilter IsSaleFilter { get; private set; }
        public string PartProductName { get; private set; } = "";

        public async Task<IActionResult> OnGetAsync(string companyName, ProductSorting sorting = ProductSorting.Default, IsSaleFilter saleFilter = IsSaleFilter.All, string partProductName = "", int pageIndex = 1) {
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

            Sorting = sorting;
            IsSaleFilter = saleFilter;
            PartProductName = partProductName;
            CurrentPage = pageIndex;
            
            const int maxCountItemsOnPage = 20;
            TotalElementsCount = await _productRepository.ProductCount(saleFilter, partProductName);
            Products = await _productRepository.GetProducts(sorting, saleFilter, partProductName, pageIndex - 1, maxCountItemsOnPage);

            return Page();
        }
    }
}