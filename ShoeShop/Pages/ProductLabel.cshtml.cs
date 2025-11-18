using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace ShoeShop.Pages
{
    [Authorize(Roles = "Admin")]
    public class ProductLabelModel : PageModel
    {
        private readonly IProductRepository repository;
        private readonly IProductStockRepository stockRepository;

        public ProductLabelModel(IProductRepository repository, IProductStockRepository stockRepository)
        {
            this.repository = repository;
            this.stockRepository = stockRepository;
        }

        public Product? Product { get; set; }
        public int TotalStock { get; set; }
        public Dictionary<int, int> SizeStocks { get; set; } = new();
        public string? QRCodeBase64 { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid productId)
        {
            Product = await repository.GetProduct(productId);
            
            if (Product == null)
            {
                return NotFound();
            }

            // Получаем общий остаток товара
            TotalStock = await stockRepository.GetTotalQuantityAsync(productId);
            
            // Получаем остатки по размерам
            var availableSizes = GetAvailableSizes(Product.Sizes);
            foreach (var size in availableSizes)
            {
                SizeStocks[size] = await stockRepository.GetQuantityAsync(productId, size);
            }

            // Генерируем QR-код для товара
            var productUrl = $"https://jxpc5n7p-7002.euw.devtunnels.ms/Product/{productId}";
            QRCodeBase64 = GenerateQRCode(productUrl);

            return Page();
        }

        private string GenerateQRCode(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var qrCodeImage = qrCode.GetGraphic(10);
            
            using var stream = new MemoryStream();
            qrCodeImage.Save(stream, ImageFormat.Png);
            var imageBytes = stream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }

        public List<int> GetAvailableSizes(ProductSize sizes)
        {
            var sizeList = new List<int>();
            
            for (int i = 1; i <= 64; i++)
            {
                var sizeFlag = (ProductSize)(1UL << (i - 1));
                if (sizes.HasFlag(sizeFlag))
                {
                    sizeList.Add(i);
                }
            }
            
            return sizeList;
        }

        public string GetSizesString(ProductSize sizes)
        {
            return string.Join(", ", GetAvailableSizes(sizes));
        }
    }
}