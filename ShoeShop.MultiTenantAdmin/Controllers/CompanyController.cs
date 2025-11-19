using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Data;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Services;

namespace ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Controllers {
    public class CompanyController : Controller {
        private readonly CompanyService _companyService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CompanyController(CompanyService companyService, UserManager<ApplicationUser> userManager, ApplicationDbContext context) {
            _companyService = companyService;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register() {
            if (!User.Identity.IsAuthenticated) {
                return Redirect("/Identity/Account/Login?returnUrl=/Company/Register");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(CompanyRegistrationModel model) {
            if (!User.Identity.IsAuthenticated) {
                return Redirect("/Identity/Account/Login?returnUrl=/Company/Register");
            }
            if (!ModelState.IsValid) {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return Redirect("/Identity/Account/Login?returnUrl=/Company/Register");
            }

            var existingCompany = await _companyService.GetCompanyByShortNameAsync(model.ShortName);
            if (existingCompany != null) {
                ModelState.AddModelError("ShortName", "Такое сокращенное название уже используется");
                return View(model);
            }

            var company = new Company {
                Id = Guid.NewGuid(),
                Name = model.Name,
                ShortName = model.ShortName,
                Description = model.Description,
                ContactEmail = model.ContactEmail,
                ContactPhone = model.ContactPhone
            };

            await _companyService.CreateCompanyAsync(company);

            var companyUser = new CompanyUser {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                UserId = user.Id,
                Role = "Admin"
            };

            _context.CompanyUsers.Add(companyUser);
            await _context.SaveChangesAsync();

            return Redirect($"/{company.ShortName}");
        }
    }

    public class CompanyRegistrationModel {
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
    }
}
