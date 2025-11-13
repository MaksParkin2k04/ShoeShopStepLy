using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShoeShop.Infrastructure;
using ShoeShop.Models;

namespace ShoeShop.Services {
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

            Product addProduct = Product.Create(product.Name, product.IsSale.Value, product.Price.Value, product.Sizes.Value,  DateTime.Now, product.Description, product.Content);
            foreach (EditImage productImage in product.Images ?? Enumerable.Empty<EditImage>()) {
                string fileName = Guid.NewGuid().ToString() + ".jpg";
                string relativePath = IMAGE_FOLDER_PATH + fileName;

                addProduct.AddImage(relativePath, productImage.Alt);
            }

            await repository.AddProduct(addProduct);

            for (int i = 0; i < addProduct.Images?.Count; i++) {
                ProductImage image = addProduct.Images[i];
                using (Stream readStream = product.Images[i].Image.OpenReadStream()) {
                    string absolutePath = Path.Combine(environment.WebRootPath, image.Path);
                    imageManager.Create(readStream, absolutePath, IMAGE_WIDTH, IMAGE_HEIGHT);
                }
            }

            return addProduct.Id;
        }

        public async Task<Guid> Update(EditProduct product) {

            product.Validate();

            Product? oldProduct = await repository.GetProduct(product.Id.Value);
            if (oldProduct == null) { throw new ArgumentNullException(nameof(product), "Товар несуществует"); }

            oldProduct.SetName(product.Name);
            oldProduct.SetIsSale(product.IsSale.Value);
            oldProduct.SetPrice(product.Price.Value);
            oldProduct.SetDescription(product.Description);
            oldProduct.SetContent(product.Content);

            Dictionary<string, IFormFile?> dictionary = new Dictionary<string, IFormFile?>();

            foreach (EditImage editImage in product.Images ?? Enumerable.Empty<EditImage>()) {

                string relativePath = IMAGE_FOLDER_PATH + Guid.NewGuid().ToString() + ".jpg";
                ProductImage? oldImage = oldProduct.Images?.FirstOrDefault(i => i.Id == editImage.Id);
                if (oldImage == null) { throw new ArgumentOutOfRangeException(nameof(product), $"Изображение Id-{editImage.Id} несуществует"); }

                switch (editImage.Mode) {
                    case EditImageMode.Original:
                        oldProduct.UpdateImageAlt(editImage.Id.Value, editImage.Alt);
                        break;
                    case EditImageMode.New:
                        oldProduct.AddImage(relativePath, editImage.Alt);
                        dictionary.Add(Path.Combine(environment.WebRootPath, relativePath), editImage.Image);
                        break;
                    case EditImageMode.Edit:
                        imageManager.Delete(Path.Combine(environment.WebRootPath, oldImage.Path));
                        oldProduct.RemoveImage(oldImage.Id);

                        dictionary.Add(Path.Combine(environment.WebRootPath, relativePath), editImage.Image);
                        oldProduct.AddImage(IMAGE_FOLDER_PATH + Guid.NewGuid().ToString(), editImage.Alt);
                        break;
                    case EditImageMode.Deleted:
                        imageManager.Delete(Path.Combine(environment.WebRootPath, oldImage!.Path));
                        oldProduct.RemoveImage(editImage.Id.Value);
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
