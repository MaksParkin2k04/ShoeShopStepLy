using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;
using System.Text.Json;

namespace ShoeShop.Pages
{
    public class PaymentWebhookModel : PageModel
    {
        private readonly IProductRepository _repository;

        public PaymentWebhookModel(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                var webhook = JsonSerializer.Deserialize<YooKassaWebhook>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

                if (webhook?.Event == "payment.succeeded" && webhook.Object?.Status == "succeeded")
                {
                    if (Guid.TryParse(webhook.Object.Metadata?.OrderId, out var orderId))
                    {
                        var order = await _repository.GetOrder(orderId);
                        if (order != null && order.Status == OrderStatus.Created)
                        {
                            order.SetStatus(OrderStatus.Paid);
                            await _repository.UpdateOrder(order);
                        }
                    }
                }

                return new OkResult();
            }
            catch
            {
                return new BadRequestResult();
            }
        }
    }

    public class YooKassaWebhook
    {
        public string Event { get; set; }
        public YooKassaWebhookObject Object { get; set; }
    }

    public class YooKassaWebhookObject
    {
        public string Status { get; set; }
        public YooKassaWebhookMetadata Metadata { get; set; }
    }

    public class YooKassaWebhookMetadata
    {
        public string OrderId { get; set; }
    }
}