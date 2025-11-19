using Microsoft.EntityFrameworkCore;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Data;
using ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Models;

namespace ShoeShop.MultiTenantAdmin.MultiTenantAdmin.Services {
    public class CompanyService {
        private readonly ApplicationDbContext _context;

        public CompanyService(ApplicationDbContext context) {
            _context = context;
        }

        public async Task<Company?> GetCompanyByShortNameAsync(string shortName) {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.ShortName == shortName && c.IsActive);
        }

        public async Task<Company> CreateCompanyAsync(Company company) {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<bool> IsUserCompanyAdminAsync(Guid userId, Guid companyId) {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId && 
                               cu.Role == "Admin" && cu.IsActive);
        }

        public async Task<List<Company>> GetUserCompaniesAsync(Guid userId) {
            return await _context.CompanyUsers
                .Where(cu => cu.UserId == userId && cu.IsActive)
                .Include(cu => cu.Company)
                .Select(cu => cu.Company)
                .ToListAsync();
        }
    }
}
