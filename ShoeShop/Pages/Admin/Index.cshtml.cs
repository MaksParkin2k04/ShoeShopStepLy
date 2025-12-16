using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Attributes;

namespace ShoeShop.Pages.Admin {
    [Authorize]
    [AdminAuth("Admin", "Manager", "Editor", "Consultant", "Analyst")]
    public class AdminIndexModel : PageModel {
        public bool CanViewOrders => User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Consultant");
        public bool CanViewProducts => User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Editor");
        public bool CanViewCategories => User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Editor");
        public bool CanViewStock => User.IsInRole("Admin") || User.IsInRole("Manager");
        public bool CanAddProducts => User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Editor");
        public bool CanViewStatistics => User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Analyst");
        public bool CanViewPromoCodes => User.IsInRole("Admin") || User.IsInRole("Manager");
        public bool CanViewAnalytics => User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Analyst");

        public bool CanViewUsers => User.IsInRole("Admin");
        public bool CanViewChat => User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Consultant");
        public bool CanViewEmailCampaigns => User.IsInRole("Admin") || User.IsInRole("Manager");
        public bool CanViewTestData => User.IsInRole("Admin");
        public bool CanViewLabelPrinting => User.IsInRole("Admin") || User.IsInRole("Manager");
        
        public string UserRole {
            get {
                if (User.IsInRole("Admin")) return "Администратор";
                if (User.IsInRole("Manager")) return "Менеджер";
                if (User.IsInRole("Editor")) return "Редактор";
                if (User.IsInRole("Consultant")) return "Консультант";
                if (User.IsInRole("Analyst")) return "Аналитик";
                return "Пользователь";
            }
        }
        
        public void OnGet() {
        }
        
        public IActionResult OnPostLogout() {
            HttpContext.Session.Remove("AdminAuth");
            return RedirectToPage("/Index");
        }
    }
}