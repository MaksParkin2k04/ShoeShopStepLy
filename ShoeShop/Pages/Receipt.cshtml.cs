using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Pages
{
    public class ReceiptModel : PageModel
    {
        private readonly IProductRepository _repository;

        public ReceiptModel(IProductRepository repository)
        {
            _repository = repository;
        }

        public Order? Order { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid orderId)
        {
            Order = await _repository.GetOrder(orderId);
            
            if (Order == null)
            {
                return NotFound();
            }
            
            // Проверяем, что заказ оплачен
            if (!Order.PaymentDate.HasValue && Order.Status != OrderStatus.Paid)
            {
                return Forbid(); // Возвращаем 403 Forbidden
            }

            TotalAmount = (decimal)Order.OrderDetails.Sum(x => x.Price);
            
            // Здесь можно добавить логику расчета скидки из промокода
            DiscountAmount = 0; // Пока без скидки
            
            var deliveryCost = TotalAmount >= 5000 ? 0 : 500;
            FinalAmount = TotalAmount - DiscountAmount + deliveryCost;

            return Page();
        }
    }
}