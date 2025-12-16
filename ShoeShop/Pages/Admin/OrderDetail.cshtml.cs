using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Pages.Admin {
    [Authorize(Roles = "Admin,Manager,Consultant")]
    public class OrderDetailModel : PageModel {
        
        public bool CanEditOrder => User.IsInRole("Admin") || User.IsInRole("Manager");
        public OrderDetailModel(IAdminRepository repository, ApplicationContext context, IEmailSender emailSender) {
            this.repository = repository;
            _context = context;
            _emailSender = emailSender;
        }

        private readonly IAdminRepository repository;
        private readonly ApplicationContext _context;
        private readonly IEmailSender _emailSender;

        public Order? Order { get; private set; }

        public async Task OnGetAsync(string orderId) {
            Order = await repository.GetOrder(orderId);
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(string orderId, int status) {
            Order? order = await repository.GetOrder(orderId);
            if (order != null) {
                var oldStatus = order.Status;
                var newStatus = (OrderStatus)status;
                order.SetStatus(newStatus);
                await repository.UpdateOrder(order);
                
                if (oldStatus != newStatus) {
                    await SendOrderStatusNotification(order, newStatus);
                    TempData["Success"] = "Статус изменен, уведомление отправлено";
                }
            }
            return RedirectToPage("/Admin/OrderDetail", new { orderId = orderId });
        }
        
        private async Task SendOrderStatusNotification(Order order, OrderStatus newStatus) {
            try {
                var user = await _context.Users.FindAsync(order.CustomerId);
                if (user == null || string.IsNullOrEmpty(user.Email)) return;
                
                var paymentStatus = order.PaymentDate.HasValue || order.Status == OrderStatus.Paid ? "Оплачен" : "Ожидает оплаты";
                var paymentColor = order.PaymentDate.HasValue || order.Status == OrderStatus.Paid ? "#28a745" : "#ffc107";
                var totalAmount = order.OrderDetails?.Sum(d => d.Price) ?? 0;
                
                var statusText = newStatus switch {
                    OrderStatus.Created => "создан",
                    OrderStatus.Paid => "оплачен",
                    OrderStatus.Processing => "обрабатывается",
                    OrderStatus.AwaitingShipment => "ожидает отправления",
                    OrderStatus.Shipped => "отправлен",
                    OrderStatus.InTransit => "в пути",
                    OrderStatus.Arrived => "прибыл в пункт выдачи",
                    OrderStatus.ReadyForPickup => "готов к выдаче",
                    OrderStatus.Completed => "доставлен",
                    OrderStatus.Returned => "возвращен",
                    OrderStatus.Canceled => "отменен",
                    _ => "изменен"
                };
                
                var subject = $"✅ Статус заказа #{order.OrderNumber} изменен";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif; background-color: #f8f9fa;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f8f9fa; padding: 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 0 20px rgba(0,0,0,0.08);'>
                    <!-- Header -->
                    <tr>
                        <td style='background-color: #ffffff; padding: 30px 30px 20px 30px; border-bottom: 3px solid #007bff;'>
                            <h1 style='margin: 0; color: #212529; font-size: 32px; font-weight: 700;'>StepLy</h1>
                            <p style='margin: 5px 0 0 0; color: #6c757d; font-size: 14px;'>Магазин обуви</p>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <h2 style='margin: 0 0 10px 0; color: #212529; font-size: 24px; font-weight: 600;'>Здравствуйте, {user.FirstName ?? user.Email}!</h2>
                            <p style='margin: 0 0 30px 0; color: #6c757d; font-size: 16px;'>Статус вашего заказа изменился</p>
                            
                            <div style='background-color: #e7f3ff; border: 1px solid #b3d9ff; padding: 20px; margin: 20px 0; border-radius: 8px;'>
                                <table width='100%' cellpadding='0' cellspacing='0'>
                                    <tr>
                                        <td style='padding-bottom: 10px;'>
                                            <span style='color: #495057; font-size: 14px;'>Заказ №</span>
                                        </td>
                                        <td align='right' style='padding-bottom: 10px;'>
                                            <span style='color: #212529; font-size: 18px; font-weight: 700;'>#{order.OrderNumber}</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding-bottom: 10px;'>
                                            <span style='color: #495057; font-size: 14px;'>Статус</span>
                                        </td>
                                        <td align='right' style='padding-bottom: 10px;'>
                                            <span style='background-color: #007bff; color: #ffffff; padding: 6px 12px; border-radius: 4px; font-size: 14px; font-weight: 600;'>{statusText.ToUpper()}</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <span style='color: #495057; font-size: 14px;'>Оплата</span>
                                        </td>
                                        <td align='right'>
                                            <span style='background-color: {paymentColor}; color: #ffffff; padding: 6px 12px; border-radius: 4px; font-size: 14px; font-weight: 600;'>{paymentStatus}</span>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            
                            <h3 style='margin: 30px 0 15px 0; color: #212529; font-size: 18px; font-weight: 600;'>Детали заказа</h3>
                            <table width='100%' cellpadding='0' cellspacing='0' style='border: 1px solid #dee2e6; border-radius: 8px; overflow: hidden;'>
                                <tr style='background-color: #f8f9fa;'>
                                    <td style='padding: 12px 15px; border-bottom: 1px solid #dee2e6;'>
                                        <span style='color: #6c757d; font-size: 13px; font-weight: 600;'>Дата заказа</span>
                                    </td>
                                    <td align='right' style='padding: 12px 15px; border-bottom: 1px solid #dee2e6;'>
                                        <span style='color: #212529; font-size: 14px;'>{order.CreatedDate:dd.MM.yyyy HH:mm}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 15px; border-bottom: 1px solid #dee2e6;'>
                                        <span style='color: #6c757d; font-size: 13px; font-weight: 600;'>Получатель</span>
                                    </td>
                                    <td align='right' style='padding: 12px 15px; border-bottom: 1px solid #dee2e6;'>
                                        <span style='color: #212529; font-size: 14px;'>{order.Recipient.Name}</span>
                                    </td>
                                </tr>
                                <tr style='background-color: #f8f9fa;'>
                                    <td style='padding: 12px 15px; border-bottom: 1px solid #dee2e6;'>
                                        <span style='color: #6c757d; font-size: 13px; font-weight: 600;'>Телефон</span>
                                    </td>
                                    <td align='right' style='padding: 12px 15px; border-bottom: 1px solid #dee2e6;'>
                                        <span style='color: #212529; font-size: 14px;'>{order.Recipient.Phone}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 15px; border-bottom: 1px solid #dee2e6;'>
                                        <span style='color: #6c757d; font-size: 13px; font-weight: 600;'>Адрес доставки</span>
                                    </td>
                                    <td align='right' style='padding: 12px 15px; border-bottom: 1px solid #dee2e6;'>
                                        <span style='color: #212529; font-size: 14px;'>{order.Recipient.City}, {order.Recipient.Street}, д. {order.Recipient.House}, кв. {order.Recipient.Apartment}</span>
                                    </td>
                                </tr>
                                <tr style='background-color: #f8f9fa;'>
                                    <td style='padding: 12px 15px;'>
                                        <span style='color: #212529; font-size: 15px; font-weight: 700;'>Итого</span>
                                    </td>
                                    <td align='right' style='padding: 12px 15px;'>
                                        <span style='color: #212529; font-size: 18px; font-weight: 700;'>{totalAmount:N0} ₽</span>
                                    </td>
                                </tr>
                            </table>
                            
                            <div style='background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; margin: 30px 0 0 0; border-radius: 8px;'>
                                <p style='margin: 0; color: #856404; font-size: 14px; line-height: 1.6;'>
                                    <strong>ℹ️ Вопросы?</strong><br>
                                    Свяжитесь с нами: <strong>+7 (999) 123-45-67</strong>
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #212529; padding: 30px; text-align: center;'>
                            <p style='margin: 0 0 5px 0; color: #ffffff; font-size: 16px; font-weight: 600;'>С уважением, команда StepLy</p>
                            <p style='margin: 0 0 20px 0; color: #adb5bd; font-size: 13px;'>Качественная обувь для всей семьи</p>
                            <p style='margin: 0; color: #6c757d; font-size: 12px;'>
                                © 2024 StepLy. Все права защищены.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
                ";
                
                await _emailSender.SendEmailAsync(user.Email, subject, body);
            } catch {
                // Игнорируем ошибки отправки
            }
        }
        
        public async Task<IActionResult> OnPostMarkAsPaidAsync(string orderId) {
            Order? order = await repository.GetOrder(orderId);
            if (order != null) {
                order.SetStatus(OrderStatus.Paid);
                await repository.UpdateOrder(order);
            }
            return RedirectToPage("/Admin/OrderDetail", new { orderId = orderId });
        }
        
        public async Task<IActionResult> OnPostAddCommentAsync(string orderId, string comment) {
            if (!string.IsNullOrEmpty(comment)) {
                Order? order = await repository.GetOrder(orderId);
                if (order != null) {
                    order.AddAdminComment(comment);
                    await repository.UpdateOrder(order);
                }
            }
            return RedirectToPage("/Admin/OrderDetail", new { orderId = orderId });
        }
    }
}