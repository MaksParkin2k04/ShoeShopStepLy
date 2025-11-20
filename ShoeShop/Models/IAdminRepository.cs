using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace ShoeShop.Models {
    public interface IAdminRepository {
        Task<Product?> GetProduct(Guid productId);
        Task<IReadOnlyList<Product>> GetProducts(ProductSorting sorting, IsSaleFilter filter, string searchName, int start, int count);
        Task<int> ProductCount(IsSaleFilter filter, string partProductName);

        Task AddProduct(Product product);
        Task UpdateProduct(Product product);
        Task RemoveProduct(Guid productId);

        Task<IReadOnlyList<Order>> GetOrders(OrderStatusFilter filter, OrderSorting sorting, int start, int count);
        Task<int> OrderCount(OrderStatusFilter filter);
        Task<Dictionary<OrderStatus, int>> GetOrderStatsByStatus(OrderStatusFilter filter);
        Task<Order?> GetOrder(string orderId);
        Task UpdateOrder(Order order);
        Task DeleteOrder(string orderId);
        Task DeleteAllOrders();
        
        // Быстрые методы с кешированием
        Task<IEnumerable<Order>> GetOrdersFast(OrderStatusFilter filter, OrderSorting sorting, int skip, int take);
        Task<int> OrderCountFast(OrderStatusFilter filter);
        Task<Dictionary<OrderStatus, int>> GetOrderStatsCache();
        Task<Order?> GetOrderByNumber(string orderNumber);

        Task<IEnumerable<Category>> GetCategories();
        Task AddCategory(Category category);
        Task RemoveCategory(Guid categoryId);
    }
}
