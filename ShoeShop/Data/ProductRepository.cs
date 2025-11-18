using Microsoft.EntityFrameworkCore;
using ShoeShop.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ShoeShop.Data {
    public class ProductRepository : IProductRepository {

        public async Task<IEnumerable<Product>> GetProducts(IReadOnlyCollection<Guid> productIds) {
            return await context.Products.Where(p => productIds.Contains(p.Id)).Include(p => p.Images).Include(p => p.Category).ToArrayAsync();
        }

        public ProductRepository(ApplicationContext context) {
            this.context = context;
        }

        private readonly ApplicationContext context;

        public async Task<IEnumerable<Product>> GetProducts(ProductSorting sorting, int start, int count) {
            IQueryable<Product> query = context.Products.IsSaleFilters(IsSaleFilter.IsSale).OrderProductsBy(sorting).Page(start, count).Include(p => p.Images).Include(p => p.Category);
            string sql = query.ToQueryString();
            return await query.ToArrayAsync();
        }

        public async Task<Product?> GetProduct(Guid productId) {
            return await context.Products.Include(p => p.Images).Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<Product?> GetByIdAsync(Guid productId) {
            return await GetProduct(productId);
        }

        public async Task<IEnumerable<Product>> GetAllAsync() {
            return await context.Products.Include(p => p.Images).Include(p => p.Category).ToArrayAsync();
        }

        public async Task<int> ProductCount() {
            return await context.Products.IsSaleFilters(IsSaleFilter.IsSale).CountAsync();
        }

        public async Task<IEnumerable<Order>> GetOrders(Guid customerId, OrderStatusFilter filter, OrderSorting sorting, int start, int count) {
            IQueryable<Order> query = context.Orders.CustomerFilter(customerId).StatusFilter(filter).OrderByDate(sorting).Page(start, count).Include(o => o.OrderDetails);
            string sql = query.ToQueryString();
            return await query.ToArrayAsync();
        }

        public async Task<Order?> GetOrder(Guid orderId) {
            return await context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task CreateOrder(Order order) {
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        public async Task UpdateOrder(Order order) {
            Order? old = await context.Orders.FirstOrDefaultAsync(o => o.Id == order.Id);
            context.Entry(old).CurrentValues.SetValues(order);
            await context.SaveChangesAsync();
        }

        public async Task<int> OrderCount(Guid customerId, OrderStatusFilter filter) {
            return await context.Orders.CustomerFilter(customerId).StatusFilter(filter).CountAsync();
        }
    }
}


