using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Attributes;
using ShoeShop.Models;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace ShoeShop.Pages.Admin
{
    [Authorize]
    [AdminAuth]
    public class PrintMultipleOrdersModel : PageModel
    {
        private readonly IAdminRepository repository;

        public PrintMultipleOrdersModel(IAdminRepository repository)
        {
            this.repository = repository;
        }

        public List<Order> Orders { get; set; } = new();
        private Dictionary<string, string> qrCodes = new();

        public async Task<IActionResult> OnGetAsync(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return NotFound();
            }

            // Парсим фильтр
            if (!Enum.TryParse<OrderStatusFilter>(filter, out var statusFilter))
            {
                return NotFound();
            }

            // Получаем все заказы по статусу с деталями
            var orders = await repository.GetOrders(statusFilter, OrderSorting.ByDateDesc, 0, 1000);
            Orders = orders.ToList();

            if (!Orders.Any())
            {
                return NotFound();
            }

            // Генерируем QR-коды для всех заказов
            foreach (var order in Orders)
            {
                var trackingUrl = $"https://jxpc5n7p-7002.euw.devtunnels.ms/OrderTracking/Track/{order.Id}";
                qrCodes[order.Id] = GenerateQRCode(trackingUrl);
            }

            return Page();
        }

        public string GetQRCodeForOrder(string orderId)
        {
            return qrCodes.GetValueOrDefault(orderId, "");
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