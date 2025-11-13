using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Data {
    public class AdminRepository : IAdminRepository {

        public AdminRepository(ApplicationContext context) {
            this.context = context;
        }

        private readonly ApplicationContext context;

        public async Task<Product?> GetProduct(Guid productId) {
            return await context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<IReadOnlyList<Product>> GetProducts(ProductSorting sorting, IsSaleFilter filter, string partProductName, int start, int count) {
            IQueryable<Product> query = context.Products.IsSaleFilters(filter).SearchByName(partProductName).OrderProductsBy(sorting).Page(start, count).Include(p => p.Images);
            string sql = query.ToQueryString();
            return await query.ToArrayAsync();
        }

        public async Task<int> ProductCount(IsSaleFilter filter, string partProductName) {
            return await context.Products.IsSaleFilters(filter).SearchByName(partProductName).CountAsync();
        }

        public async Task AddProduct(Product product) {
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product product) {
            context.Update(product);
            int a = await context.SaveChangesAsync();
        }

        public async Task RemoveProduct(Guid productId) {
            Product? product = await GetProduct(productId);
            if (product != null) {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<Order>> GetOrders(OrderStatusFilter filter, OrderSorting sorting, int start, int count) {
            IQueryable<Order> query = context.Orders.StatusFilter(filter).OrderByDate(sorting).Page(start, count).Include(o => o.OrderDetails);
            return await query.ToArrayAsync();
        }

        public async Task<int> OrderCount(OrderStatusFilter filter) {
            return await context.Orders.StatusFilter(filter).CountAsync();
        }
    }
}
