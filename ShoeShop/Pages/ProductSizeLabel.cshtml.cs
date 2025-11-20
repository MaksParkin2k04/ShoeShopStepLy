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
    public class ProductSizeLabelModel : PageModel
    {
        private readonly IProductRepository repository;
        private readonly IProductStockRepository stockRepository;

        public ProductSizeLabelModel(IProductRepository repository, IProductStockRepository stockRepository)
        {
            this.repository = repository;
            this.stockRepository = stockRepository;
        }

        public Product? Product { get; set; }
        public int Size { get; set; }
        public int StockQuantity { get; set; }
        public string? QRCodeBase64 { get; set; }
        public int Count { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(Guid productId, int size, int count = 1)
        {
            Product = await repository.GetProduct(productId);
            Size = size;
            
            if (Product == null)
            {
                return NotFound();
            }

            // Получаем остаток товара по размеру
            StockQuantity = await stockRepository.GetQuantityAsync(productId, size);

            // Генерируем QR-код для товара с размером
            var productUrl = $"https://jxpc5n7p-7002.euw.devtunnels.ms/Product/{productId}";
            QRCodeBase64 = GenerateQRCode(productUrl);
            Count = Math.Max(1, Math.Min(50, count)); // Ограничиваем от 1 до 50

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
    }
}