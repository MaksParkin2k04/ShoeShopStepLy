namespace ShoeShop.Models {
    public interface IProductRepository {
        Task<IEnumerable<Product>> GetProducts(IReadOnlyCollection<Guid> productIds);
        Task<IEnumerable<Product>> GetProducts(ProductSorting sorting, int start, int count);
        Task<Product?> GetProduct(Guid productId);
        Task<Product?> GetByIdAsync(Guid productId);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<int> ProductCount();

        Task<IEnumerable<Order>> GetOrders(Guid customerId, OrderStatusFilter filter, OrderSorting sorting, int start, int count);
        Task<Order?> GetOrder(Guid orderId);
        Task CreateOrder(Order order);
        Task UpdateOrder(Order order);
        Task<int> OrderCount(Guid customerId, OrderStatusFilter filter);
    }
}
