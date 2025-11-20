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
    public class OrderLabelModel : PageModel
    {
        private readonly IAdminRepository adminRepository;

        public OrderLabelModel(IAdminRepository adminRepository)
        {
            this.adminRepository = adminRepository;
        }

        public ShoeShop.Models.Order? Order { get; set; }
        public string? QRCodeBase64 { get; set; }

        public async Task<IActionResult> OnGetAsync(string orderId)
        {
            Order = await adminRepository.GetOrder(orderId);
            
            if (Order == null)
            {
                return NotFound();
            }

            // Генерируем QR-код
            var orderUrl = $"https://jxpc5n7p-7002.euw.devtunnels.ms/OrderTracking/Track/{orderId}";
            QRCodeBase64 = GenerateQRCode(orderUrl);

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