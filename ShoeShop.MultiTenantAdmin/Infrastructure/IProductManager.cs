using ShoeShop.MultiTenantAdmin.Infrastructure;

namespace ShoeShop.MultiTenantAdmin.Models {
    public interface IProductManager {
        Task<Guid> Add(EditProduct product);
        Task<Guid> Update(EditProduct product);
        Task Delete(Guid productId);
    }
}
