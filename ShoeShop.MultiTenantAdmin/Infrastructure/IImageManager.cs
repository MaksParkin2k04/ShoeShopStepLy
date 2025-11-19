namespace ShoeShop.MultiTenantAdmin.Infrastructure {
    public interface IImageManager {
        void Create(Stream readStream, string writeFileePath, int width, int height);
        void Delete(string filePath);
    }
}
