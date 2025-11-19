using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.MultiTenantAdmin.Models;
using ShoeShop.MultiTenantAdmin.Services;
using System.ComponentModel.DataAnnotations;

namespace ShoeShop.MultiTenantAdmin.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class PromoCodesModel : PageModel
    {
        private readonly PromoCodeService _promoCodeService;

        public PromoCodesModel(PromoCodeService promoCodeService)
        {
            _promoCodeService = promoCodeService;
        }

        public List<PromoCode> PromoCodes { get; set; } = new();

        [BindProperty]
        public PromoCodeInputModel NewPromoCode { get; set; } = new();

        public class PromoCodeInputModel
        {
            [Required]
            [StringLength(20)]
            public string Code { get; set; } = string.Empty;

            [Required]
            [Range(0.01, 100)]
            public decimal DiscountPercent { get; set; }

            [Range(0.01, double.MaxValue)]
            public decimal? MaxDiscountAmount { get; set; }

            public DateTime? ExpiresAt { get; set; }

            [Range(1, int.MaxValue)]
            public int? UsageLimit { get; set; }
        }

        public async Task OnGetAsync()
        {
            PromoCodes = await _promoCodeService.GetAllPromoCodesAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                PromoCodes = await _promoCodeService.GetAllPromoCodesAsync();
                return Page();
            }

            try
            {
                await _promoCodeService.CreatePromoCodeAsync(
                    NewPromoCode.Code,
                    NewPromoCode.DiscountPercent,
                    NewPromoCode.MaxDiscountAmount,
                    NewPromoCode.ExpiresAt,
                    NewPromoCode.UsageLimit
                );

                TempData["Success"] = "Промокод успешно создан!";
                return RedirectToPage();
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Ошибка при создании промокода. Возможно, код уже существует.");
                PromoCodes = await _promoCodeService.GetAllPromoCodesAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _promoCodeService.DeletePromoCodeAsync(id);
            TempData["Success"] = "Промокод удален!";
            return RedirectToPage();
        }
    }
}
