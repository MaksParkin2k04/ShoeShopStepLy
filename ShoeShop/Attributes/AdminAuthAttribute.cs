using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using ShoeShop.Data;

namespace ShoeShop.Attributes {
    public class AdminAuthAttribute : ActionFilterAttribute {
        private readonly string[] _allowedRoles;
        
        public AdminAuthAttribute(params string[] allowedRoles) {
            _allowedRoles = allowedRoles ?? new[] { "Admin" };
        }
        
        public override void OnActionExecuting(ActionExecutingContext context) {
            // Сначала проверяем авторизацию пользователя
            if (!context.HttpContext.User.Identity.IsAuthenticated) {
                var returnUrl = context.HttpContext.Request.Path;
                context.Result = new RedirectToPageResult("/Account/Login", new { ReturnUrl = returnUrl });
                return;
            }
            
            // Проверяем роли пользователя
            var hasRequiredRole = false;
            foreach (var role in _allowedRoles) {
                if (context.HttpContext.User.IsInRole(role)) {
                    hasRequiredRole = true;
                    break;
                }
            }
            
            if (!hasRequiredRole) {
                // Если нет нужной роли, проверяем админский пароль (для обратной совместимости)
                var session = context.HttpContext.Session;
                if (session.GetString("AdminAuth") != "true") {
                    context.Result = new RedirectToPageResult("/AccessDenied");
                    return;
                }
            }
        }
    }
}