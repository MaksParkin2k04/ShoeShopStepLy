using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Data;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Services;
using System.ComponentModel.DataAnnotations;

namespace ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Pages {
    [Authorize]
    public class CompanyRegisterModel : PageModel {
        private readonly CompanyService _companyService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CompanyRegisterModel(CompanyService companyService, UserManager<ApplicationUser> userManager, ApplicationDbContext context) {
            _companyService = companyService;
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel {
            [Required]
            public string Name { get; set; } = string.Empty;
            
            [Required]
            public string ShortName { get; set; } = string.Empty;
            
            public string? Description { get; set; }
            
            [Required]
            [EmailAddress]
            public string ContactEmail { get; set; } = string.Empty;
            
            public string? ContactPhone { get; set; }
        }

        public void OnGet() {
        }

        public async Task<IActionResult> OnPostAsync() {
            if (!ModelState.IsValid) {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return RedirectToPage("/Account/Login");
            }

            var existingCompany = await _companyService.GetCompanyByShortNameAsync(Input.ShortName);
            if (existingCompany != null) {
                ModelState.AddModelError("Input.ShortName", "Такое сокращенное название уже используется");
                return Page();
            }

            var company = new Company {
                Id = Guid.NewGuid(),
                Name = Input.Name,
                ShortName = Input.ShortName,
                Description = Input.Description,
                ContactEmail = Input.ContactEmail,
                ContactPhone = Input.ContactPhone
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
}
