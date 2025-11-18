namespace ShoeShop.Models {
    public interface ICustomerRepository {
        Task AddOrderAsync(Order order);
        Task<ICollection<Order>> GetOrdersByTelegramIdAsync(string telegramId);
    }
}
