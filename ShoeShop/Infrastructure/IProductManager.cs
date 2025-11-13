using ShoeShop.Infrastructure;

namespace ShoeShop.Models {
    public interface IProductManager {
        Task<Guid> Add(EditProduct product);
        Task<Guid> Update(EditProduct product);
        Task Delete(Guid productId);
    }
}
