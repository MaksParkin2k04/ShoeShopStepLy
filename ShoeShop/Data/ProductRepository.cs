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

        public async Task<Order?> GetOrder(string orderId) {
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
        
        public async Task<(IEnumerable<Product> products, int totalCount)> GetProductsPagedAsync(ProductSorting sorting, int pageIndex, int pageSize, Guid? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null, int[]? sizes = null) {
            var query = context.Products.IsSaleFilters(IsSaleFilter.IsSale).Include(p => p.Images).Include(p => p.Category).AsQueryable();
            
            if (categoryId.HasValue) {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
            
            if (minPrice.HasValue) {
                query = query.Where(p => p.Price >= (double)minPrice.Value);
            }
            
            if (maxPrice.HasValue) {
                query = query.Where(p => p.Price <= (double)maxPrice.Value);
            }
            
            if (sizes != null && sizes.Length > 0) {
                foreach (var size in sizes) {
                    var sizeFlag = (ProductSize)Enum.Parse(typeof(ProductSize), $"S{size}");
                    query = query.Where(p => p.Sizes.HasFlag(sizeFlag));
                }
            }
            
            var totalCount = await query.CountAsync();
            var products = await query.OrderProductsBy(sorting).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            
            return (products, totalCount);
        }
        
        public async Task<List<Product>> GetProductsBatchAsync(List<Guid> productIds) {
            return await context.Products.Where(p => productIds.Contains(p.Id)).Include(p => p.Images).Include(p => p.Category).ToListAsync();
        }
    }
}


