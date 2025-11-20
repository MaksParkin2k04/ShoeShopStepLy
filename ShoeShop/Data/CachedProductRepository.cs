using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop.Data
{
    public class CachedProductRepository : IProductRepository
    {
        private readonly IProductRepository _repository;
        private readonly ICacheService _cache;
        private const int CACHE_MINUTES = 10;

        public CachedProductRepository(ProductRepository repository, ICacheService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<IEnumerable<Product>> GetProducts(IReadOnlyCollection<Guid> productIds)
        {
            var cacheKey = $"products_by_ids_{string.Join(",", productIds)}";
            var cached = await _cache.GetAsync<IEnumerable<Product>>(cacheKey);
            
            if (cached != null)
                return cached;

            var products = await _repository.GetProducts(productIds);
            await _cache.SetAsync(cacheKey, products, TimeSpan.FromMinutes(CACHE_MINUTES));
            
            return products;
        }

        public async Task<IEnumerable<Product>> GetProducts(ProductSorting sorting, int start, int count)
        {
            var cacheKey = $"products_{sorting}_{start}_{count}";
            var cached = await _cache.GetAsync<IEnumerable<Product>>(cacheKey);
            
            if (cached != null)
                return cached;

            var products = await _repository.GetProducts(sorting, start, count);
            await _cache.SetAsync(cacheKey, products, TimeSpan.FromMinutes(CACHE_MINUTES));
            
            return products;
        }

        public Task<Product?> GetProduct(Guid productId) => _repository.GetProduct(productId);
        public Task<Product?> GetByIdAsync(Guid productId) => _repository.GetByIdAsync(productId);
        
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            var cacheKey = "all_products";
            var cached = await _cache.GetAsync<IEnumerable<Product>>(cacheKey);
            
            if (cached != null)
                return cached;

            var products = await _repository.GetAllAsync();
            await _cache.SetAsync(cacheKey, products, TimeSpan.FromMinutes(CACHE_MINUTES));
            
            return products;
        }

        public async Task<int> ProductCount()
        {
            var cacheKey = "product_count";
            var cached = await _cache.GetAsync<int?>(cacheKey);
            
            if (cached.HasValue)
                return cached.Value;

            var count = await _repository.ProductCount();
            await _cache.SetAsync(cacheKey, count, TimeSpan.FromMinutes(CACHE_MINUTES));
            
            return count;
        }

        // Order methods without caching
        public Task<IEnumerable<Order>> GetOrders(Guid customerId, OrderStatusFilter filter, OrderSorting sorting, int start, int count) => _repository.GetOrders(customerId, filter, sorting, start, count);
        public Task<Order?> GetOrder(string orderId) => _repository.GetOrder(orderId);
        public Task CreateOrder(Order order) => _repository.CreateOrder(order);
        public Task UpdateOrder(Order order) => _repository.UpdateOrder(order);
        public Task<int> OrderCount(Guid customerId, OrderStatusFilter filter) => _repository.OrderCount(customerId, filter);
    }
}