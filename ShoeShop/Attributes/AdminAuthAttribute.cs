using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ShoeShop.Attributes {
    public class AdminAuthAttribute : ActionFilterAttribute {
        public override void OnActionExecuting(ActionExecutingContext context) {
            // Сначала проверяем авторизацию пользователя
            if (!context.HttpContext.User.Identity.IsAuthenticated) {
                var returnUrl = context.HttpContext.Request.Path;
                context.Result = new RedirectToPageResult("/Account/Login", new { ReturnUrl = returnUrl });
                return;
            }
            
            // Затем проверяем админский пароль
            var session = context.HttpContext.Session;
            if (session.GetString("AdminAuth") != "true") {
                var returnUrl = context.HttpContext.Request.Path;
                context.Result = new RedirectToPageResult("/AdminAuth", new { returnUrl });
            }
        }
    }
}