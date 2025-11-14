using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShoeShop.Data;
using ShoeShop.Data.Initialization;
using ShoeShop.Infrastructure;
using ShoeShop.Models;
using ShoeShop.Services;

namespace ShoeShop {
    public class Program {
        public static async Task Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic }));

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена.");
            builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationContext>();



            builder.Services.AddSession();
            builder.Services.AddRazorPages();
            builder.Services.AddScoped<IImageManager, ImageManager>();
            builder.Services.AddScoped<IProductManager, ProductManager>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IBasketShoppingService, BasketShoppingCookies>();

            builder.Services.Configure<IdentityOptions>(options => {
                // Параметры для требований к паролю..
                options.Password.RequireDigit = false;            // Возвращает или задает флаг, указывающий, должны ли пароли содержать цифру. По умолчанию используется значение 'true'.
                options.Password.RequireLowercase = false;        // Возвращает или задает флаг, указывающий, должны ли пароли содержать символ ASCII в нижнем регистре. По умолчанию используется значение 'true'.
                options.Password.RequireNonAlphanumeric = false;  // Возвращает или задает флаг, указывающий, должны ли пароли содержать не буквенно-цифровой символ. По умолчанию используется значение 'true'.
                options.Password.RequireUppercase = false;        // Возвращает или задает флаг, указывающий, должны ли пароли содержать символ ASCII в верхнем регистре. По умолчанию используется значение 'true'.
                options.Password.RequiredLength = 5;              // Возвращает или задает минимальную длину пароля. Значение по умолчанию — 6.
                options.Password.RequiredUniqueChars = 1;         // Возвращает или задает минимальное количество уникальных символов, которое должен содержать пароль. По умолчанию равен 1.

                // Параметры для блокировки пользователей..
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Возвращает или задает флаг, указывающий, можно ли заблокировать нового пользователя. По умолчанию имеет значение true.
                options.Lockout.MaxFailedAccessAttempts = 5;                      // Возвращает или задает число неудачных попыток доступа, разрешенных до блокировки пользователя при условии, что блокировка включена. Значение по умолчанию — 5.
                options.Lockout.AllowedForNewUsers = true;                        // Возвращает или задает флаг, указывающий, можно ли заблокировать нового пользователя. По умолчанию имеет значение true.

                // Параметры проверки пользователей.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Возвращает или задает список допустимых символов в имени пользователя, используемом для проверки имен пользователей. По умолчанию — abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+
                options.User.RequireUniqueEmail = true;                                                                         // Возвращает или задает флаг, указывающий, требуется ли приложению уникальные сообщения электронной почты для пользователей. Значение по умолчанию — false.
            });

            WebApplication app = builder.Build();

            // Инициализация базы данных
            //using (IServiceScope scope = app.Services.CreateScope())
            //{
            //    ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            //    IUserStore<ApplicationUser> userStore = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
            //    UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            //    IRoleStore<ApplicationRole> roleStore = scope.ServiceProvider.GetRequiredService<IRoleStore<ApplicationRole>>();
            //    RoleManager<ApplicationRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            //    await DatabaseInitializer.Initialize(context, userStore, userManager, roleStore, roleManager);
            //}

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseMigrationsEndPoint();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapRazorPages();

            app.Run();
        }
    }
}
