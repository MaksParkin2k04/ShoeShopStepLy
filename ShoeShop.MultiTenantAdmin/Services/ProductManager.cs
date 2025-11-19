using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShoeShop.MultiTenantAdmin.Infrastructure;
using ShoeShop.MultiTenantAdmin.Models;

namespace ShoeShop.MultiTenantAdmin.Services {
    public class ProductManager : IProductManager {

        private const int IMAGE_WIDTH = 600;
        private const int IMAGE_HEIGHT = 800;
        private const string IMAGE_FOLDER_PATH = "images/products/";

        public ProductManager(IWebHostEnvironment environment, IAdminRepository repository, IImageManager imageManager) {
            this.environment = environment;
            this.repository = repository;
            this.imageManager = imageManager;
        }

        private readonly IWebHostEnvironment environment;
        private readonly IAdminRepository repository;
        private readonly IImageManager imageManager;

        public async Task<Guid> Add(EditProduct product) {
            product.Validate();

            Product newProduct = Product.Create(
                product.Name,
                product.IsSale.Value,
                product.Price.Value,
                product.Sizes.Value,
                DateTime.Now,
                product.Description,
                product.Content,
                product.CategoryId ?? Guid.Empty
            );

            Dictionary<string, IFormFile?> dictionary = new Dictionary<string, IFormFile?>();

            foreach (EditImage editImage in product.Images ?? Enumerable.Empty<EditImage>()) {
                if (editImage.Mode == EditImageMode.New && editImage.Image != null) {
                    string relativePath = IMAGE_FOLDER_PATH + Guid.NewGuid().ToString() + ".jpg";
                    newProduct.AddImage(relativePath, editImage.Alt);
                    dictionary.Add(Path.Combine(environment.WebRootPath, relativePath), editImage.Image);
                }
            }

            await repository.AddProduct(newProduct);

            foreach (string imagePath in dictionary.Keys) {
                IFormFile? formFile = dictionary[imagePath];
                if (formFile != null) {
                    using (Stream stream = formFile.OpenReadStream()) {
                        imageManager.Create(stream, imagePath, IMAGE_WIDTH, IMAGE_HEIGHT);
                    }
                }
            }

            return newProduct.Id;
        }

        public async Task<Guid> Update(EditProduct product) {

            product.Validate();

            Product? oldProduct = await repository.GetProduct(product.Id.Value);
            if (oldProduct == null) { throw new ArgumentNullException(nameof(product), "Товар несуществует"); }

            oldProduct.SetName(product.Name);
            oldProduct.SetIsSale(product.IsSale.Value);
            oldProduct.SetPrice(product.Price.Value);
            oldProduct.SetSalePrice(product.SalePrice);
            oldProduct.SetDescription(product.Description);
            oldProduct.SetContent(product.Content);
            if (product.Sizes.HasValue) {
                oldProduct.SetSizes(product.Sizes.Value);
            }
            if (product.CategoryId.HasValue) {
                oldProduct.SetCategory(product.CategoryId.Value);
            }

            Dictionary<string, IFormFile?> dictionary = new Dictionary<string, IFormFile?>();

            foreach (EditImage editImage in product.Images ?? Enumerable.Empty<EditImage>()) {
                string relativePath = IMAGE_FOLDER_PATH + Guid.NewGuid().ToString() + ".jpg";
                
                switch (editImage.Mode) {
                    case EditImageMode.New:
                        if (editImage.Image != null) {
                            oldProduct.AddImage(relativePath, editImage.Alt);
                            dictionary.Add(Path.Combine(environment.WebRootPath, relativePath), editImage.Image);
                        }
                        break;
                    case EditImageMode.Original:
                    case EditImageMode.Edit:
                    case EditImageMode.Deleted:
                        if (editImage.Id.HasValue) {
                            ProductImage? oldImage = oldProduct.Images?.FirstOrDefault(i => i.Id == editImage.Id);
                            if (oldImage != null) {
                                switch (editImage.Mode) {
                                    case EditImageMode.Original:
                                        oldProduct.UpdateImageAlt(editImage.Id.Value, editImage.Alt);
                                        break;
                                    case EditImageMode.Edit:
                                        imageManager.Delete(Path.Combine(environment.WebRootPath, oldImage.Path));
                                        oldProduct.RemoveImage(oldImage.Id);
                                        if (editImage.Image != null) {
                                            dictionary.Add(Path.Combine(environment.WebRootPath, relativePath), editImage.Image);
                                            oldProduct.AddImage(relativePath, editImage.Alt);
                                        }
                                        break;
                                    case EditImageMode.Deleted:
                                        imageManager.Delete(Path.Combine(environment.WebRootPath, oldImage.Path));
                                        oldProduct.RemoveImage(editImage.Id.Value);
                                        break;
                                }
                            }
                        }
                        break;
                }
            }

            await repository.UpdateProduct(oldProduct);

            foreach (string imagePath in dictionary.Keys) {
                IFormFile? formFile = dictionary[imagePath];

                using (Stream stream = formFile?.OpenReadStream()) {
                    imageManager.Create(stream, imagePath, IMAGE_WIDTH, IMAGE_HEIGHT);
                }
            }

            return oldProduct.Id;
        }

        public async Task Delete(Guid productId) {
            Product? product = await repository.GetProduct(productId);
            if (product == null) { throw new ArgumentNullException(nameof(productId), "Товар несуществует"); }

            foreach (ProductImage image in product.Images ?? Enumerable.Empty<ProductImage>()) {
                string filePath = Path.Combine(environment.WebRootPath, image.Path);
                imageManager.Delete(filePath);
            }

            await repository.RemoveProduct(productId);
        }
    }
}
